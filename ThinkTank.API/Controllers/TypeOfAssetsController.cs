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
        /// Get list type of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AssetResponse>>> GetTypeOfAssets([FromQuery] PagingRequest pagingRequest, [FromQuery] TypeOfAssetRequest assetRequest)
        {
            var rs = await _assetService.GetTypeOfAssets(assetRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssetResponse>> GetTypeOfAssetsById(int id)
        {
            var rs = await _assetService.GetTypeOfAssetById(id);
            return Ok(rs);
        }

    }
}
