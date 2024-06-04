

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommand:ICommand<AccountResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int UserId { get; }
        public UpdateAccountRequest UpdateAccountRequest { get; }
        public UpdateAccountCommand(int userId, UpdateAccountRequest updateAccountRequest)
        {
            UserId = userId;
            UpdateAccountRequest = updateAccountRequest;
        }
    }
}
