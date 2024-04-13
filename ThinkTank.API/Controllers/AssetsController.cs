using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/assets")]
    [ApiController]
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Get list of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "All")]
        [HttpGet]
        public async Task<ActionResult<List<AssetResponse>>> GetAssets([FromQuery] PagingRequest pagingRequest, [FromQuery] AssetRequest assetRequest)
        {
            var rs = await _assetService.GetAssets(assetRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssetResponse>> GetAssetsById(int id)
        {
            var rs = await _assetService.GetAssetById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create asset  
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
     [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<List<AssetResponse>>> CreateAsset([FromBody] List<CreateAssetRequest> resource)
        {
            var rs = await _assetService.CreateAsset(resource);
            return Ok(rs);
        }
        /// <summary>
        /// Delete asset
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete]
        public async Task<ActionResult<NotificationResponse>> DeleteAsset([FromBody] List<int> assetId)
        {
            var rs = await _assetService.DeleteAsset(assetId);
            return Ok(rs);
        }
    }
}
