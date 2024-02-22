using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/challenges")]
    [ApiController]
    public class ChallengesController : ControllerBase
    {
        private readonly
            IChallengeService _challengeService;

        public ChallengesController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChallengeResponse>>> GetChallenges([FromQuery] PagingRequest pagingRequest, [FromQuery] ChallengeRequest challengeRequest)
        {
            var rs = await _challengeService.GetChallenges(challengeRequest, pagingRequest);
            return Ok(rs);
        }
    }
}
