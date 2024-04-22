
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountService
    {
        Task<PagedResults<AccountResponse>> GetAccounts(AccountRequest request, PagingRequest paging);
        Task<AccountResponse> LoginPlayer(LoginRequest request);
        Task<AccountResponse> LoginAdmin(LoginRequest request);
        Task<AccountResponse> CreateAccount(CreateAccountRequest createAccountRequest);
        Task<AccountResponse> RevokeRefreshToken(int userId);
        Task<AccountResponse> GetAccountById(int id);
        Task<AccountResponse> LoginGoogle(LoginGoogleRequest request);
        Task<AccountResponse> VerifyAndGenerateToken(TokenRequest request);
        Task<AccountResponse> UpdatePass(ResetPasswordRequest request);
        Task<AccountResponse> UpdateAccount(int accountId, UpdateAccountRequest request);
        Task<AccountResponse> GetToBanAccount(int id);
        Task<AccountResponse> GetIdToLogin(LoginRequest request, string? googleId);
        Task<List<GameLevelOfAccountResponse>> GetGameLevelByAccountId(int accountId);

    }
}
