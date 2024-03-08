using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/typeOfAssets")]
    [ApiController]
    public class TypeOfAssetsController : Controller
    {
        private readonly ITypeOfAssetService _assetService;
        public TypeOfAssetsController(ITypeOfAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Get list of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AssetResponse>>> GetAssets([FromQuery] PagingRequest pagingRequest, [FromQuery] TypeOfAssetRequest assetRequest)
        {
            var rs = await _assetService.GetTypeOfAssets(assetRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssetResponse>> GetTypeOfAssetsById(int id)
        {
            var rs = await _assetService.GetTypeOfAssetById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create type of asset  
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
      [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<AssetResponse>> CreateTypeOfAsset([FromBody] CreateTypeOfAssetRequest resource)
        {
            var rs = await _assetService.CreateTypeOfAsset(resource);
            return Ok(rs);
        }
        /// <summary>
        /// Update resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AssetResponse>> UpdateResource([FromBody] CreateTypeOfAssetRequest request, int id)
        {
            var rs = await _assetService.UpdateTypeOfAsset(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Delete resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<AssetResponse>> DeleteResource(int id)
        {
            var rs = await _assetService.DeleteTypeOfAsset(id);
            return Ok(rs);
        }
    }
}
