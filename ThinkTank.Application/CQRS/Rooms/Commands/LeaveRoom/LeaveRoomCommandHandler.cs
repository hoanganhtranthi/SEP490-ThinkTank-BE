

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Commands.LeaveRoom
{
    public class LeaveRoomCommandHandler : ICommandHandler<LeaveRoomCommand, RoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly ISlackService _slackService;
        public LeaveRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _slackService = slackService;
        }

        public async Task<RoomResponse> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                .Include(x => x.Topic.Game).FirstOrDefault(u => u.Id == request.RoomId);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with id {request.RoomId}", "");
                }
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} Not Found!!!!!", "");

                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Available!!!!!", "");
                }

                var accountInRoom = _unitOfWork.Repository<AccountInRoom>().Find(x => x.AccountId == request.AccountId && x.RoomId == request.RoomId);
                if (accountInRoom == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"This account does not participate in the room {request.RoomId}", "");

                if (room.StartTime != null && room.StartTime < date)
                    throw new CrudException(HttpStatusCode.BadRequest, $"The room has started so {request.AccountId} can't leave", "");

                _unitOfWork.Repository<AccountInRoom>().Delete(accountInRoom);
                await _unitOfWork.CommitAsync();

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
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Leave room error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Leave room error!!!!!", ex.Message);
            }
        }
    }
}
