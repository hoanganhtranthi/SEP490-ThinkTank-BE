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
        private readonly ITypeOfAssetService _typeOfAssetService;
        public TypeOfAssetsController(ITypeOfAssetService typeOfAssetService)
        {
            _typeOfAssetService = typeOfAssetService;
        }

        /// <summary>
        /// Get list type of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="typeOfAssetRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<TypeOfAssetResponse>>> GetTypeOfAssets([FromQuery] PagingRequest pagingRequest, [FromQuery] TypeOfAssetRequest typeOfAssetRequest)
        {
            var rs = await _typeOfAssetService.GetTypeOfAssets(typeOfAssetRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TypeOfAssetResponse>> GetTypeOfAssetsById(int id)
        {
            var rs = await _typeOfAssetService.GetTypeOfAssetById(id);
            return Ok(rs);
        }

    }
}
