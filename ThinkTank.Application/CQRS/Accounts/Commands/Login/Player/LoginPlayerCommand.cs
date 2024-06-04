using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.Login
{
    public class LoginPlayerCommand : ICommand<AccountResponse>
    {
        public LoginRequest LoginRequest { get; }
        public LoginPlayerCommand(LoginRequest loginRequest)
        {
            LoginRequest = loginRequest;
        }
    }
}
