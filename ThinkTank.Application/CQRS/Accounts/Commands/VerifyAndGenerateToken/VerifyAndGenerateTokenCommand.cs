

using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.VerifyAndGenerateToken
{
    public class VerifyAndGenerateTokenCommand:ICommand<AccountResponse>
    {
        public TokenRequest TokenRequest { get; }
        public VerifyAndGenerateTokenCommand(TokenRequest tokenRequest)
        {
            TokenRequest = tokenRequest;
        }
    }
}
