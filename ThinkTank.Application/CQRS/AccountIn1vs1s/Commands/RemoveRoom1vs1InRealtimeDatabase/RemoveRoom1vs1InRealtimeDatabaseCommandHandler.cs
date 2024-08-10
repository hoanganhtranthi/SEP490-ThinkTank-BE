

using System.Net;

using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.RemoveRoom1vs1InRealtimeDatabase
{
    public class RemoveRoom1vs1InRealtimeDatabaseCommandHandler : ICommandHandler<RemoveRoom1vs1InRealtimeDatabaseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        private readonly ISlackService _slackService;
        public RemoveRoom1vs1InRealtimeDatabaseCommandHandler(IUnitOfWork unitOfWork, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
            _slackService = slackService;
        }

        public async Task<bool> Handle(RemoveRoom1vs1InRealtimeDatabaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var room1vs1 = _unitOfWork.Repository<AccountIn1vs1>().Find(x => x.RoomOfAccountIn1vs1Id == request.Room1vs1Id);
                if (room1vs1 == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Room Id {request.Room1vs1Id} is not found", "");

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<dynamic>($"battle/{request.Room1vs1Id}");
                await Task.Delay(request.DelayTime * 1000);

                if (roomRealtimeDatabase == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Room Id {request.Room1vs1Id} has already deleted", "");

                return await _firebaseRealtimeDatabaseService.RemoveDataFlutterRealtimeDatabase($"battle/{request.Room1vs1Id}");
            }
            catch (CrudException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Remove room 1vs1 in realtime database error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Remove room 1vs1 in realtime database error!!!", ex.InnerException?.Message);
            }
        }
    }
}
