
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Queries.GetAccountById
{
    public class GetAccountByIdQuery : IGetTByIdQuery<AccountResponse>
    {
        public GetAccountByIdQuery(int id) : base(id)
        {
        }
    }
}
