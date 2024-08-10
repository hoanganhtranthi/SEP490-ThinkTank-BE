

using AutoMapper;
using Firebase.Auth;
using System.Net;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Commands.Logout
{
    public class RevokeRefreshTokenCommandHandler : ICommandHandler<RevokeRefreshTokenCommand, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        private readonly ISlackService _slackService;
        public RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                AccountResponse rs = new AccountResponse();
                if (request.UserId == 0)
                {
                    var adminAccountResponse = _firebaseRealtimeDatabaseService.GetAsync<AdminAccountResponse>("AdminAccount").Result;
                    if (adminAccountResponse != null)
                    {
                        adminAccountResponse.VersionTokenAdmin += 1;
                        adminAccountResponse.RefreshTokenAdmin = null;
                        await _firebaseRealtimeDatabaseService.SetAsync<AdminAccountResponse>("AdminAccount", adminAccountResponse);
                    }
                    rs.UserName = "Admin";
                }
                else
                {
                    Account account = _unitOfWork.Repository<Account>().GetAsync(a => a.Id == request.UserId).Result;
                    if (account == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {request.UserId}", "");
                    }

                    if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                    account.RefreshToken = null;
                    account.Fcm = null;
                    account.VersionToken = account.VersionToken + 1;

                    await _unitOfWork.Repository<Account>().Update(account, account.Id);
                    await _unitOfWork.CommitAsync();
                    rs = _mapper.Map<Account, AccountResponse>(account);
                }
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Revoke RefreshToken Fail"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Revoke RefreshToken Fail", ex.InnerException?.Message);
            }
        }
    }
}
