using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.TypeOfAssets.Queries.GetTypeOfAssets
{
    public class GetTypeOfAssetsQuery : IGetTsQuery<PagedResults<TypeOfAssetResponse>>
    {
        public GetTypeOfAssetsQuery(PagingRequest pagingRequest) : base(pagingRequest)
        {
        }
    }
}
