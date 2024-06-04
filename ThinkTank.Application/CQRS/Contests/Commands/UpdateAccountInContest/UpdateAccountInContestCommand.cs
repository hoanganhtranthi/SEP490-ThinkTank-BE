

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Commands.UpdateAccountInContest
{
    public class UpdateAccountInContestCommand:ICommand<AccountInContestResponse>
    {
        public UpdateAccountInContestRequest UpdateAccountInContestRequest { get; }
        public UpdateAccountInContestCommand(UpdateAccountInContestRequest updateAccountInContestRequest)
        {
            UpdateAccountInContestRequest = updateAccountInContestRequest;
        }
    }
}
