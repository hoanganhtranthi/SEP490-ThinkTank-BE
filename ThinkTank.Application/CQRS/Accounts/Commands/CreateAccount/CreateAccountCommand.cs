
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.CreateAccount
{
    public class CreateAccountCommand:ICommand<AccountResponse>
    {
        public CreateAccountRequest CreateAccountRequest { get; }
        public CreateAccountCommand(CreateAccountRequest createAccountRequest)
        {
            CreateAccountRequest = createAccountRequest;
        }
    }
}
