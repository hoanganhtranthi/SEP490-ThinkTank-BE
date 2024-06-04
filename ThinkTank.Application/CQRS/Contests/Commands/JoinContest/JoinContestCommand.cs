

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Commands.JoinContest
{
    public class JoinContestCommand:ICommand<AccountInContestResponse>
    {
        public CreateAccountInContestRequest CreateAccountInContestRequest { get; }
        public JoinContestCommand(CreateAccountInContestRequest createAccountInContestRequest)
        {
            CreateAccountInContestRequest = createAccountInContestRequest;
        }
    }
}
