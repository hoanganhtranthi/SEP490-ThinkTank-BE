using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/typeOfAssetInContests")]
    [ApiController]
    public class TypeOfAssetInContestsController : Controller
    {
        private readonly ITypeOfAssetInContestService _typeOfAssetInContestService;
        public TypeOfAssetInContestsController(ITypeOfAssetInContestService typeOfAssetInContestService)
        {
            _typeOfAssetInContestService = typeOfAssetInContestService;
        }

        /// <summary>
        /// Get list of type assets in contest
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="typeAssetInContestRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<TypeOfAssetInContestResponse>>> GetAssetInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] TypeOfAssetInContestRequest typeAssetInContestRequest)
        {
            var rs = await _typeOfAssetInContestService.GetTypeOfAssetInContests(typeAssetInContestRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset in contest  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TypeOfAssetInContestResponse>> GetTypeOfAssetsInContestById(int id)
        {
            var rs = await _typeOfAssetInContestService.GetTypeOfAssetInContestById(id);
            return Ok(rs);
        }
    }
}
