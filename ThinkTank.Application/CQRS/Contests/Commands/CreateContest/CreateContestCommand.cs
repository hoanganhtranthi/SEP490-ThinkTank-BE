
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.UnitOfWork;

namespace ThinkTank.Application.CQRS.Contests.Commands.CreateContest
{
    public class CreateContestCommand:ICommand<ContestResponse>
    {
        public CreateAndUpdateContestRequest CreateAndUpdateContest { get; }
        public CreateContestCommand(CreateAndUpdateContestRequest createAndUpdateContest)
        {
            CreateAndUpdateContest = createAndUpdateContest;
        }
    }
}
