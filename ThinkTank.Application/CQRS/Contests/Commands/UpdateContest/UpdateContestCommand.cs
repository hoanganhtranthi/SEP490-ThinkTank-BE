

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Commands.UpdateContest
{
    public class UpdateContestCommand:ICommand<ContestResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public CreateAndUpdateContestRequest CreateAndUpdateContest { get; }
        public UpdateContestCommand(CreateAndUpdateContestRequest createAndUpdateContest,int id)
        {
            CreateAndUpdateContest = createAndUpdateContest;
            Id = id;
        }
    }
}
