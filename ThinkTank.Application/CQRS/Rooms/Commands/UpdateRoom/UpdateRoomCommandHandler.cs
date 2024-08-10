

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand, RoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); //chỉ cho phép một luồng truy cập vào critical section tại một thời điểm
        private readonly DateTime date;
        private readonly ISlackService _slackService;
        public UpdateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,ISlackService slackService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _slackService = slackService;
        }

        public async Task<RoomResponse> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync();
                try
                {

                    var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.Topic.Game).Include(x => x.AccountInRooms).SingleOrDefault(x => x.Code == request.RoomCode);

                    if (room == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Room Code {request.RoomCode} Not Found!!!!!", "");

                    if (room.Status == false)
                        throw new CrudException(HttpStatusCode.BadRequest, "The room has ended so you can't update", "");

                    if (room.StartTime != null && room.StartTime < date)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Accounts in this room party have already started playing so cannot be canceled", "");

                    List<AccountInRoom> list = new List<AccountInRoom>();
                    list = room.AccountInRooms.ToList();

                    foreach (var accountInRoom in request.CreateAndUpdateAccountInRoomRequests)
                    {
                        if (accountInRoom.AccountId <= 0 || accountInRoom.Duration < 0 || accountInRoom.Mark < 0 || accountInRoom.PieceOfInformation < 0)
                            throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                        var account = _mapper.Map<AccountInRoom>(accountInRoom);

                        var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == accountInRoom.AccountId);
                        if (acc == null)
                            throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountInRoom.AccountId} Not Found!!!!!", "");

                        if (acc.Status.Equals(false))
                        {
                            throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountInRoom.AccountId} Not Available!!!!!", "");
                        }

                        var accInRoom = _unitOfWork.Repository<AccountInRoom>().Find(x => x.AccountId == accountInRoom.AccountId && x.RoomId == room.Id);
                        if (accInRoom != null)
                            throw new CrudException(HttpStatusCode.BadRequest, $"The account id {accInRoom.AccountId} that joined this room", "");

                        account.Room = room;
                        account.IsAdmin = false;
                        account.RoomId = room.Id;
                        list.Add(account);
                    }
                    room.AccountInRooms = list;

                    if (room.AccountInRooms.Count() > room.AmountPlayer)
                        throw new CrudException(HttpStatusCode.BadRequest, "The number of participants does not match the amout player", "");

                    await _unitOfWork.Repository<Room>().Update(room, room.Id);
                    await _unitOfWork.CommitAsync();

                    var rs = _mapper.Map<RoomResponse>(room);
                    rs.GameName = room.Topic.Game.Name;
                    rs.TopicName = room.Topic.Name;
                    rs.AccountInRoomResponses = _mapper.Map<List<AccountInRoomResponse>>(room.AccountInRooms);

                    foreach (var acc in rs.AccountInRoomResponses)
                    {
                        acc.Avatar = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).Avatar;
                        acc.Username = _unitOfWork.Repository<Account>().Find(x => x.Id == acc.AccountId).UserName;
                    }


                    return rs;
                }
                finally
                {
                    _semaphore.Release();

                    //try...finally:    
                    // để đảm bảo rằng tài nguyên được giải phóng sau khi không còn cần thiết kể cả khi có ngoại lệ xảy ra
                    // Điều này giúp tránh được deadlock vì tài nguyên sẽ luôn được giải phóng dù có lỗi xảy ra trong quá trình thực thi hay không.
                }

            }

            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Add Member To Room Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Member To Room Error!!!", ex?.Message);
            }
        }
    }
}
