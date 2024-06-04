

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetAccountInContests
{
    public class GetAccountInContestsQuery:IGetTsQuery<PagedResults<AccountInContestResponse>>
    {
        public AccountInContestRequest AccountInContestRequest { get; }
        public GetAccountInContestsQuery(PagingRequest pagingRequest,AccountInContestRequest accountInContestRequest) : base(pagingRequest)
        {
            AccountInContestRequest = accountInContestRequest;
        }
       
    }
}
