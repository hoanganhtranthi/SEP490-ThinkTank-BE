

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Assets.Queries.GetAssets
{
    public class GetAssetsQuery:IGetTsQuery<PagedResults<AssetResponse>>
    {
        public AssetRequest AssetRequest { get; }
        public GetAssetsQuery(PagingRequest pagingRequest,AssetRequest assetRequest) : base(pagingRequest)
        {
            AssetRequest = assetRequest;
        }
        
    }
}
