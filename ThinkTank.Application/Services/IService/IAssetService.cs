

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IAssetService
    {
        Task<PagedResults<AssetResponse>> GetAssets(AssetRequest request, PagingRequest paging);
        Task<AssetResponse> GetAssetById(int id);
        Task<List<AssetResponse>> CreateAsset(List<CreateAssetRequest> request);
        Task<List<AssetResponse>> DeleteAsset(List<int> request);
    }
}
