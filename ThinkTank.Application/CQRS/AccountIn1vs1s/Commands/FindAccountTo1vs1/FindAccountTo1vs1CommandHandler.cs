

using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.AccountIn1vs1s.DomainServices;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.FindAccountTo1vs1
{
    public class FindAccountTo1vs1CommandHandler : ICommandHandler<FindAccountTo1vs1Command, RoomIn1vs1Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); //chỉ cho phép một luồng truy cập vào critical section tại một thời điểm
        private readonly ISlackService _slackService;
        public FindAccountTo1vs1CommandHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<RoomIn1vs1Response> Handle(FindAccountTo1vs1Command request, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync();
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

                    var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.GameId);
                    if (game == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Game Id {request.GameId} Not Found!!!!!", "");

                    var accountId = 0;
                    var uniqueId = "";
                    var accountFind = new Account();
                    var queue = await CacheService.Instance.GetJobsAsync("account1vs1");
                    if (queue?.Any() == true)
                    {
                        foreach (var accountInfo in queue)
                        {
                            var accountValues = accountInfo.Split('+');

                            var accountIdFromCache = Int32.Parse(accountValues[0]);
                            var coinFromCache = Int32.Parse(accountValues[1]);
                            var gameIdFromCache = Int32.Parse(accountValues[2]);

                            //check xem account da vao queue hay chua . Neu vao queue bao loi : Da vao queue roi   
                            if (coinFromCache == request.Coin && gameIdFromCache == request.GameId && accountIdFromCache == request.AccountId)
                                throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} has been added to the queue", "");

                            //Neu chua vao queue xem queue do co ai phu hop voi coin va gameId do hay khong
                            //Neu co thi get ra
                            if (coinFromCache == request.Coin && gameIdFromCache == request.GameId)
                            {
                                accountId = accountIdFromCache;
                                accountFind = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);
                                await CacheService.Instance.DeleteJobAsync("account1vs1", accountInfo); // Xóa dữ liệu khỏi cache sau khi tìm thấy tài khoản
                                uniqueId = accountValues[3];
                                break;
                            }
                        }

                        //Neu chua vao queue thi them vao queue
                        if (accountId == 0)
                        {
                            uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                            var cacheKey = $"{request.AccountId}+{request.Coin}+{request.GameId}+{uniqueId}";
                            await CacheService.Instance.AddJobAsync(cacheKey, "account1vs1");
                        }
                    }

                    //Neu queue dang rong thi them account nay vao
                    else
                    {
                        uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var cacheKey = $"{request.AccountId}+{request.Coin}+{request.GameId}+{uniqueId}";
                        await CacheService.Instance.AddJobAsync(cacheKey, "account1vs1");
                    }
                    return new RoomIn1vs1Response
                    {
                        AccountId = accountId,
                        Avatar = accountFind.Avatar==null?null:accountFind.Avatar,
                        Coin =(int) accountFind.Coin,
                        RoomId = uniqueId,
                        Username = accountFind.UserName==null?null:accountFind.UserName
                    };
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
                throw;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Find account to play 1vs1 Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Find account to play 1vs1 Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
