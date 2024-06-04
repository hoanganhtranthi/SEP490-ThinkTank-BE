
using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.Logout
{
    public class RevokeRefreshTokenCommand:ICommand<AccountResponse>
    {
        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int UserId { get; }
        public RevokeRefreshTokenCommand(int userId) { UserId = userId; }
    }
}
