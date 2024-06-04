

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.BanAccount
{
    public class BanAccountCommandHandler : ICommandHandler<BanAccountCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        public BanAccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<AccountResponse> Handle(BanAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Account account = _unitOfWork.Repository<Account>()
                    .Find(c => c.Id == request.Id);

                if (account == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {request.Id}", "");
                }

                //Check xem account đó có đang online hay không
                if (CheckIsLogin(request.Id).Result == true)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.Id} is online so it cannot be banned ", "");

                account.Status = !account.Status;
                account.VersionToken += 1;
                account.RefreshToken = null;

                await _unitOfWork.Repository<Account>().Update(account, request.Id);
                await _unitOfWork.CommitAsync();

                return _mapper.Map<Account, AccountResponse>(account);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Ban account error!!!!!", ex.Message);
            }
        }
        private async Task<bool> CheckIsLogin(int id)
        {
                Account user = new Account();
                var result = false;
                user = _unitOfWork.Repository<Account>().Find(u => u.Id == id);
                if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "Account Not Found", "");

                //Get trạng thái của account hiện tại là online hay offline
                var isOnline = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}");
                if (isOnline != null)
                {
                    //Set trạng thái lại là offline
                    await _firebaseRealtimeDatabaseService.SetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}", false);

                    Thread.Sleep(2000);

                    //Get ra trạng thái của account hiện tại  sau khi deplay 2s
                    result = await _firebaseRealtimeDatabaseService.GetAsyncOfFlutterRealtimeDatabase<bool>($"/islogin/{id}");
                }
                return result;
            }
        }
}
