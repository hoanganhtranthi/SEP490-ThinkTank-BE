using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.Login
{
    public class LoginAdminCommand : ICommand<AccountResponse>
    {
        public LoginRequest LoginRequest { get; }
        public LoginAdminCommand(LoginRequest loginRequest)
        {
            LoginRequest = loginRequest;
        }
    }
}
