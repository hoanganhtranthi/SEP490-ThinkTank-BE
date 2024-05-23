
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IAccountInContestService
    {
        Task<PagedResults<AccountInContestResponse>> GetAccountInContests(AccountInContestRequest request, PagingRequest paging);
        Task<AccountInContestResponse> GetAccountInContestById(int id);
        Task<AccountInContestResponse> UpdateAccountInContest(UpdateAccountInContestRequest request);
        Task<AccountInContestResponse> CreateAccountInContest(CreateAccountInContestRequest request);
    }
}
