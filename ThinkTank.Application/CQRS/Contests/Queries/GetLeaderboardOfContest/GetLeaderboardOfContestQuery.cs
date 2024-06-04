

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetLeaderboardOfContest
{
    public class GetLeaderboardOfContestQuery:IQuery<PagedResults<LeaderboardResponse>>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public PagingRequest PagingRequest { get; }
        public GetLeaderboardOfContestQuery(int id, PagingRequest pagingRequest)
        {
            Id = id;
            PagingRequest = pagingRequest;
        }
    }
}
