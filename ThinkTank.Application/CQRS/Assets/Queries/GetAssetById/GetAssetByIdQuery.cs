
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Assets.Queries.GetAssetById
{
    public class GetAssetByIdQuery : IGetTByIdQuery<AssetResponse>
    {
        public GetAssetByIdQuery(int id) : base(id)
        {
        }
    }
}
