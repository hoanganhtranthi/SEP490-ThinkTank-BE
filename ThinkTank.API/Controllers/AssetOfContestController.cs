using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/assetOfContest")]
    [ApiController]
    public class AssetOfContestController : ControllerBase
    {
        private readonly
            IAssetOfContestService _assetOfContestService;

        public AssetOfContestController(IAssetOfContestService assetOfContestService)
        {
            _assetOfContestService = assetOfContestService;
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<AssetOfContestResponse>> CreateAssetOfContest([FromBody] CreateAssetOfContestRequest request)
        {
            var rs = await _assetOfContestService.CreateAssetOfContest(request);
            return Ok(rs);
        }
    }
}
