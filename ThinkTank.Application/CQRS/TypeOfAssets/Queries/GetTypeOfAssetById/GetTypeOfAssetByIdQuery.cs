
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.TypeOfAssets.Queries.GetTypeOfAssetById
{
    public class GetTypeOfAssetByIdQuery : IGetTByIdQuery<TypeOfAssetResponse>
    {
        public GetTypeOfAssetByIdQuery(int id) : base(id)
        {
        }
    }
}
