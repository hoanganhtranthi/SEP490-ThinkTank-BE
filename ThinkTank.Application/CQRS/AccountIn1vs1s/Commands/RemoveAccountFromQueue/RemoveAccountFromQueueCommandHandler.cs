

using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.CQRS.AccountIn1vs1s.DomainServices;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Queries.RemoveAccountFromQueue
{
    public class RemoveAccountFromQueueCommandHandler : IQueryHandler<RemoveAccountFromQueueCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public RemoveAccountFromQueueCommandHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<bool> Handle(RemoveAccountFromQueueCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var account = await _unitOfWork.Repository<Account>().FindAsync(a => a.Id == request.AccountId);
                if (account == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} Not Found!!!!!", "");
                }

                if (account.Status == false)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Available!!!!!", "");
                }

                var game = await _unitOfWork.Repository<Game>().FindAsync(x => x.Id == request.GameId);

                if (game == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Game Id {request.GameId} not found", "");

                var list = await CacheService.Instance.GetJobsAsync("account1vs1");
                if (list == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Found In Cache!!!!!", "");

                string acc = list.SingleOrDefault(x => x == $"{request.AccountId}+{request.Coin}+{request.GameId}+{request.UniqueId}");
                if (acc == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Found In Cache!!!!!", "");

                await Task.Delay(request.DelayTime * 1000);
                await CacheService.Instance.DeleteJobAsync("account1vs1", account);

                return true;
            }
            catch (CrudException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Remove account from queue error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Remove account from queue error!!!", ex.InnerException?.Message);
            }
        }
    }
}
