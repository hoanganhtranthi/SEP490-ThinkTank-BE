

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetContests
{
    public class GetContestsQuery:IGetTsQuery<PagedResults<ContestResponse>>
    {
        public ContestRequest ContestRequest { get; }
        public GetContestsQuery(PagingRequest pagingRequest,ContestRequest contestRequest) : base(pagingRequest)
        {
            ContestRequest = contestRequest;
        }
        
    }
}
