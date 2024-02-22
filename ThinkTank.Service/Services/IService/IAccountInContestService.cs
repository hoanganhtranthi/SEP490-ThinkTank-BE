using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAccountInContestService
    {
        Task<PagedResults<AccountInContestResponse>> GetAccountInContests(AccountInContestRequest request, PagingRequest paging);
        Task<AccountInContestResponse> GetAccountInContest(AccountInContestRequest account);
        Task<AccountInContestResponse> CreateAccountInContest(CreateAccountInContestRequest request);
        Task<AccountInContestResponse> UpdateAccountInContest(int accountInContestId ,UpdateAccountInContestRequest request);
    }
}
