

using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.StartRoomIn1vs1
{
    public class StartRoomIn1vs1CommandHandler : ICommandHandler<StartRoomIn1vs1Command, bool>
    {
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        public StartRoomIn1vs1CommandHandler(IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<bool> Handle(StartRoomIn1vs1Command request, CancellationToken cancellationToken)
        {
            try
            {

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<dynamic>($"battle/{request.RoomIn1vs1Id}");
                Thread.Sleep(request.Time * 1000 + 15000);

                if (roomRealtimeDatabase != null)
                {
                    if (request.IsUser1 == true)
                        await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<int>($"battle/{request.RoomIn1vs1Id}/progress1", request.ProgressTime);
                    else await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<int>($"battle/{request.RoomIn1vs1Id}/progress2", request.ProgressTime);
                }
                return true;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Start To Play 1vs1 error!!!!!", ex.Message);
            }
        }
    }
}
