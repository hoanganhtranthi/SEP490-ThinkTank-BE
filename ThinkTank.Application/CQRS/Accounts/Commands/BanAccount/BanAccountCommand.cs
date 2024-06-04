

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Commands.BanAccount
{
    public class BanAccountCommand:ICommand<AccountResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public BanAccountCommand(int id)
        {
            Id = id;
        }
    }
}
