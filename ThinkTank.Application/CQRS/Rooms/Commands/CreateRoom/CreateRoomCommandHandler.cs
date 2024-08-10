

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, RoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public CreateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }
        public async Task<RoomResponse> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateRoomRequest.Name == null || request.CreateRoomRequest.Name == "" || request.CreateRoomRequest.AccountId <= 0 || request.CreateRoomRequest.TopicId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");
                var room = _mapper.Map<CreateRoomRequest, Room>(request.CreateRoomRequest);

                var topic = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking()
                    .Include(a => a.Game).SingleOrDefault(a => a.Id == request.CreateRoomRequest.TopicId);

                if (topic == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Topic Id {request.CreateRoomRequest.TopicId} Not Found!!!!!", "");
                }


                if (room.AmountPlayer < 2 || room.AmountPlayer > 8)
                    throw new CrudException(HttpStatusCode.BadRequest, "Amout Player Is Invalid", "");

                room.StartTime = null;
                room.Code = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                AccountInRoom accountInRoom = new AccountInRoom();

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.CreateRoomRequest.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateRoomRequest.AccountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateRoomRequest.AccountId} Not Available!!!!!", "");
                }

                accountInRoom.AccountId = request.CreateRoomRequest.AccountId;
                accountInRoom.IsAdmin = true;
                accountInRoom.Room = room;
                room.AccountInRooms.Add(accountInRoom);
                room.Status = null;

                await _unitOfWork.Repository<Room>().CreateAsync(room);
                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<RoomResponse>(room);
                rs.GameName = topic.Game.Name;
                rs.TopicName = topic.Name;

                var accountInRoomResponse = _mapper.Map<AccountInRoomResponse>(accountInRoom);
                accountInRoomResponse.Avatar = account.Avatar;
                accountInRoomResponse.Username = account.UserName;
                rs.AccountInRoomResponses = new List<AccountInRoomResponse>();
                rs.AccountInRoomResponses.Add(accountInRoomResponse);

                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Create Room Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Room Error!!!", ex?.Message);
            }
        }
    }
}
