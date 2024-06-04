
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Rooms.Commands.RemoveRoomPartyInRealtimeDatabase
{
    public class RemoveRoomPartyInRealtimeDatabaseCommandHandler : ICommandHandler<RemoveRoomPartyInRealtimeDatabaseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        public RemoveRoomPartyInRealtimeDatabaseCommandHandler(IUnitOfWork unitOfWork,IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _unitOfWork = unitOfWork;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<bool> Handle(RemoveRoomPartyInRealtimeDatabaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var room = _unitOfWork.Repository<Room>().GetAll().Include(x => x.Topic).Include(x => x.AccountInRooms)
                   .Include(x => x.Topic.Game).FirstOrDefault(u => u.Code == request.RoomCode);

                if (room == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found room with code {request.RoomCode}", "");
                }

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<RoomRealtimeDatabaseResponse>($"room/{request.RoomCode}");
                Thread.Sleep(request.DelayTime * 1000);

                if (roomRealtimeDatabase == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Room Code {request.RoomCode} has already deleted", "");

                return await _firebaseRealtimeDatabaseService.RemoveDataFlutterRealtimeDatabase($"room/{request.RoomCode}");
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Remove Room in realtime database error!!!!!", ex.Message);
            }
        }
    }
}
