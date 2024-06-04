

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Commands.UpdateAccountInRoom
{
    public class UpdateAccountInRoomCommandHandler : ICommandHandler<UpdateAccountInRoomCommand, AccountInRoomResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        public UpdateAccountInRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
        }
        
        public async Task<AccountInRoomResponse> Handle(UpdateAccountInRoomCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAndUpdateAccountInRoomRequest.AccountId <= 0 ||request.CreateAndUpdateAccountInRoomRequest.Duration < 0 || request.CreateAndUpdateAccountInRoomRequest.Mark < 0 || request.CreateAndUpdateAccountInRoomRequest.PieceOfInformation < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == request.CreateAndUpdateAccountInRoomRequest.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateAndUpdateAccountInRoomRequest.AccountId} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateAndUpdateAccountInRoomRequest.AccountId} Not Available!!!!!", "");
                }

                var room = _unitOfWork.Repository<Room>().Find(x => x.Code == request.RoomCode);
                if (room == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This room Id {request.CreateAndUpdateAccountInRoomRequest.AccountId} is not found !!!", "");

                var accountInRoom = _unitOfWork.Repository<AccountInRoom>().GetAll().SingleOrDefault(x => x.AccountId == request.CreateAndUpdateAccountInRoomRequest.AccountId && x.RoomId == room.Id);
                if (accountInRoom == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account in room Id {request.CreateAndUpdateAccountInRoomRequest.AccountId} is not found in room {request.RoomCode}!!!", "");

                _mapper.Map<CreateAndUpdateAccountInRoomRequest, AccountInRoom>(request.CreateAndUpdateAccountInRoomRequest, accountInRoom);
                accountInRoom.CompletedTime = date;

                await _unitOfWork.Repository<AccountInRoom>().Update(accountInRoom, accountInRoom.Id);
                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<AccountInRoomResponse>(accountInRoom);
                rs.Username = a.UserName;
                rs.Avatar = a.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Account In Room Error!!!", ex?.Message);
            }
        }
    }
}
