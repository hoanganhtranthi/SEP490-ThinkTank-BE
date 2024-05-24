

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface ITypeOfAssetService
    {
        Task<PagedResults<TypeOfAssetResponse>> GetTypeOfAssets(TypeOfAssetRequest request, PagingRequest paging);
        Task<TypeOfAssetResponse> GetTypeOfAssetById(int id);
    }
}
