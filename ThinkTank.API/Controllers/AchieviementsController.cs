using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Achieviements.Commands.CreateAchievement;
using ThinkTank.Application.CQRS.Achieviements.Queries.GetLeaderboard;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/achievements")]
    [ApiController]
    public class AchieviementsController : Controller
    {
        private readonly IMediator _mediator;
        public AchieviementsController(IMediator mediator)
        {
            _mediator= mediator;
        }
        /// <summary>
        /// Add achievement
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost()]
        [ProducesResponseType(typeof(AchievementResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementRequest request)
        {
            var rs = await _mediator.Send(new CreateAchievementCommand(request));
            return Ok(rs);
        }
        /// <summary>
        /// Get lederboard of a level of the game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{gameId:int}/leaderboard")]
        [ProducesResponseType(typeof(PagedResults<LeaderboardResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLeaderboard(int gameId, [FromQuery] PagingRequest request, [FromQuery] int? accountId)
        {
            var rs = await _mediator.Send(new GetLeaderboardQuery(gameId,accountId,request));
            return Ok(rs);
        }
    }
}
