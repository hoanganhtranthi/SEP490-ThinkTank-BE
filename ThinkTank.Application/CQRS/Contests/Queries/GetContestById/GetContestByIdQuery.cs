
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetContestById
{
    public class GetContestByIdQuery : IGetTByIdQuery<ContestResponse>
    {
        public GetContestByIdQuery(int id) : base(id)
        {
        }
    }
}
