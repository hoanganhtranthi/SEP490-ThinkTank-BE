

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.ForgotPassword
{
    public class ForgotPasswordCommand:ICommand<AccountResponse>
    {
        public ResetPasswordRequest ResetPasswordRequest { get; }
        public ForgotPasswordCommand(ResetPasswordRequest resetPasswordRequest) { ResetPasswordRequest = resetPasswordRequest; }
    }
}
