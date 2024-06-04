

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Achieviements.Queries.GetLeaderboard
{
    public class GetLeaderboardQuery:IQuery<PagedResults<LeaderboardResponse>>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public PagingRequest PagingRequest { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int? AccountId { get; }
        public GetLeaderboardQuery(int id, int? accountId, PagingRequest pagingRequest)
        {
            Id = id;
            AccountId = accountId;
            PagingRequest = pagingRequest;
        }
    }
}
