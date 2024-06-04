
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Queries.GetAccounts
{
    public class GetAccountsQuery:IGetTsQuery<PagedResults<AccountResponse>>
    {
        public AccountRequest AccountRequest { get; }
        public GetAccountsQuery(PagingRequest pagingRequest, AccountRequest accountRequest) : base(pagingRequest)
        {
            AccountRequest = accountRequest;
        }

    }
}
