

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Commands.DeleteContest
{
    public class DeleteContestCommand:ICommand<ContestResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public DeleteContestCommand(int id)
        {
            Id = id;
        }
    }
}
