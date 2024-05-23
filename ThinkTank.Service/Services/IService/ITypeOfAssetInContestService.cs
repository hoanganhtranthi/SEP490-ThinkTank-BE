

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface ITypeOfAssetInContestService
    {
        Task<PagedResults<TypeOfAssetInContestResponse>> GetTypeOfAssetInContests(TypeOfAssetInContestRequest request, PagingRequest paging);
        Task<TypeOfAssetInContestResponse> GetTypeOfAssetInContestById(int id);     
    }
}
