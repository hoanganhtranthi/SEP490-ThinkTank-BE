
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetToStartRoom
{
    public class GetToStartRoomQueryHandler : IQueryHandler<GetToStartRoomQuery, RoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        private readonly ISlackService _slackService;
        public GetToStartRoomQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _mapper = mapper;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
            _slackService = slackService;
        }

        public async Task<RoomResponse> Handle(GetToStartRoomQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                    .Include(x => x.Topic.Game).FirstOrDefault(u => u.Code == request.RoomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {request.RoomCode}", "");
                }

                if (room.AccountInRooms.Count() < 2 || room.AccountInRooms.Count() > room.AmountPlayer)
                    throw new CrudException(HttpStatusCode.BadRequest, "The number of participants does not match the amout player", "");

                if (room.AccountInRooms.SingleOrDefault(x => x.AccountId == request.AccountId && x.IsAdmin == true) == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} does not have permission to start this room id", "");
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Available!!!!!", "");
                }

                room.StartTime = date;
                room.Status = true;

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<RoomRealtimeDatabaseResponse>($"room/{request.RoomCode}");
                Thread.Sleep(request.Time * 1000 + 10000);

                room.Status = false;
                room.EndTime = date.AddMilliseconds(request.Time * 1000 + 10000);

                await _unitOfWork.Repository<Room>().UpdateDispose(room, room.Id);
                await _unitOfWork.CommitAsync();


                if (roomRealtimeDatabase != null)
                {
                    roomRealtimeDatabase.AmountPlayerDone = room.AccountInRooms.Count();
                    await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<int>($"room/{request.RoomCode}/AmountPlayerDone", roomRealtimeDatabase.AmountPlayerDone);
                }
                var rs = _mapper.Map<RoomResponse>(room);
                rs.TopicName = room.Topic.Name;
                rs.GameName = room.Topic.Game.Name;
                rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);

                foreach (var acc in rs.AccountInRoomResponses)
                {
                    acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                    acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                }
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Start Room To Play error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Start Room To Play error!!!!!", ex.Message);
            }
        }
    }
}
