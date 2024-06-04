
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.LoginGoogle
{
    public class LoginGoogleCommand:ICommand<AccountResponse>
    {
        public LoginGoogleRequest LoginGoogleRequest { get; }
        public LoginGoogleCommand(LoginGoogleRequest loginGoogleRequest)
        {
            LoginGoogleRequest = loginGoogleRequest;
        }
    }
}
