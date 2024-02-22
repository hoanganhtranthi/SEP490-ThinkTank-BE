using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/prizeOfContests")]
    [ApiController]
    public class PrizeOfContestController : ControllerBase
    {
        private readonly IPrizeOfContestService _prizeOfContestService;

        public PrizeOfContestController(IPrizeOfContestService prizeOfContestService)
        {
            _prizeOfContestService = prizeOfContestService;
        }

        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<PrizeOfContestResponse>>> GetPrizeOfContests([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceOfContestRequest request)
        {
            var rs = await _prizeOfContestService.GetPrizeOfContests(request, pagingRequest);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PrizeOfContestResponse>> CreatePrizeOfContest([FromBody] CreatePrizeOfContestRequest prizeOfContestRequest)
        {
            var rs = await _prizeOfContestService.CreatePrizeOfContest(prizeOfContestRequest);
            return Ok(rs);
        }
    }
}
