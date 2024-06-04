

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Icons.Commands.BuyIcon
{
    public class BuyIconCommand:ICommand<IconOfAccountResponse>
    {
        public CreateIconOfAccountRequest CreateIconOfAccountRequest { get;  }
        public BuyIconCommand(CreateIconOfAccountRequest createIconOfAccountRequest)
        {
            CreateIconOfAccountRequest = createIconOfAccountRequest;
        }
    }
}
