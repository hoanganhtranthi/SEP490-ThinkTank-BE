using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
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
        /// <summary>
        /// Get challenges of account(1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="challengeRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpGet]
        public async Task<ActionResult<List<ChallengeResponse>>> GetChallenges([FromQuery] ChallengeRequest challengeRequest)
        {
            var rs = await _challengeService.GetChallenges(challengeRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get Coin Reward Of Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="challengeId"></param>
        /// <returns></returns>
         [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int}")]
        public async Task<ActionResult<List<ChallengeResponse>>> GetToRewardCoin(int accountId, [FromQuery] int? challengeId)
        {
            var rs = await _challengeService.GetCoinReward(accountId,challengeId);
            return Ok(rs);
        }
    }
}
