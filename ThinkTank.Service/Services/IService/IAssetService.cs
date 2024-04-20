using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAssetService
    {
        Task<PagedResults<AssetResponse>> GetAssets(AssetRequest request, PagingRequest paging);
        Task<AssetResponse> GetAssetById(int id);
        Task<List<AssetResponse>> CreateAsset(List<CreateAssetRequest> request);
        Task<List<AssetResponse>> DeleteAsset(List<int> request);
    }
}
