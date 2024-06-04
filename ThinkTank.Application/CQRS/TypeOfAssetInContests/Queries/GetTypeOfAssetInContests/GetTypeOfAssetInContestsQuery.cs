

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContests
{
    public class GetTypeOfAssetInContestsQuery:IGetTsQuery<PagedResults<TypeOfAssetInContestResponse>>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int? ContestId { get; }
        public GetTypeOfAssetInContestsQuery(PagingRequest pagingRequest,int? contestId) : base(pagingRequest)
        {
            ContestId = contestId;
        }
        
    }
}
