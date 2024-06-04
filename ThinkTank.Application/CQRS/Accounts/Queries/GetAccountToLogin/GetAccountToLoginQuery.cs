
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Queries.GetAccountToLogin
{
    public class GetAccountToLoginQuery:IQuery<AccountResponse>
    {
        public string Username { get; }
        public string Password { get; }
        public string GoogleId { get; }
        public GetAccountToLoginQuery(string username, string password, string googleId)
        {
            Username = username;
            Password = password;
            GoogleId = googleId;
        }
    }
}
