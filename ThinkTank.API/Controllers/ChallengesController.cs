using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Challenges.Commands.RewardCoin;
using ThinkTank.Application.CQRS.Challenges.Queries.GetChallenges;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/challenges")]
    [ApiController]
    public class ChallengesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChallengesController(IMediator mediator)
        {
            _mediator=mediator; 
        }
        /// <summary>
        /// Get challenges of account(1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="challengeRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpGet]
        [ProducesResponseType(typeof(List<ChallengeResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetChallenges([FromQuery] ChallengeRequest challengeRequest)
        {
            var rs = await _mediator.Send(new GetChallengesQuery(challengeRequest));
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
        [ProducesResponseType(typeof(List<ChallengeResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToRewardCoin(int accountId, [FromQuery] int? challengeId)
        {
            var rs = await _mediator.Send(new RewardCoinCommand(accountId, challengeId));
            return Ok(rs);
        }
    }
}
