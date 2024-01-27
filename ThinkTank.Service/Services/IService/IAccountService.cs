using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountService
    {
        Task<PagedResults<AccountResponse>> GetAccounts(AccountRequest request, PagingRequest paging);
        Task<dynamic> CreateMailMessage(string email);
        Task<AccountResponse> Login(LoginRequest request);
        Task<AccountResponse> CreateAccount(CreateAccountRequest createAccountRequest);
        Task<AccountResponse> RevokeRefreshToken(string userName);
        Task<AccountResponse> GetAccountById(int id);
        Task<AccountResponse> LoginGoogle(string data);
        Task<AccountResponse> VerifyAndGenerateToken(TokenRequest request);
        Task<AccountResponse> UpdatePass(ResetPasswordRequest request);
        Task<AccountResponse> UpdateAccount(int accountId, UpdateAccountRequest request);
        Task<AccountResponse> GetToUpdateStatus(int id);
        Task<AccountResponse> GetToBanAccount(int id);
    }
}
