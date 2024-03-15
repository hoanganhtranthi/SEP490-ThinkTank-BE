using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/typeOfAssetInContests")]
    [ApiController]
    public class TypeOfAssetInContestsController : Controller
    {
        private readonly ITypeOfAssetInContestService _assetService;
        public TypeOfAssetInContestsController(ITypeOfAssetInContestService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Get list of type assets in contest
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AssetOfContestResponse>>> GetAssetInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] TypeOfAssetInContestRequest assetRequest)
        {
            var rs = await _assetService.GetTypeOfAssetInContests(assetRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset in contest  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssetOfContestResponse>> GetTypeOfAssetsInContestById(int id)
        {
            var rs = await _assetService.GetTypeOfAssetInContestById(id);
            return Ok(rs);
        }
    }
}
