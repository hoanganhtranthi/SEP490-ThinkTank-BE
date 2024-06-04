
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContestById
{
    public class GetTypeOfAssetsInContestByIdQuery : IGetTByIdQuery<TypeOfAssetInContestResponse>
    {
        public GetTypeOfAssetsInContestByIdQuery(int id) : base(id)
        {
        }
    }
}
