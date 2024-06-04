using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Accounts.Queries.GetAccountById;
using ThinkTank.Application.CQRS.Games.Queries.GetGames;
using ThinkTank.Application.CQRS.Topics.Queries.GetReportOGame;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GamesController : Controller
    {
       private readonly IMediator _mediator;
        public GamesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get list of games
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<GameResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGames([FromQuery] PagingRequest pagingRequest)
        {
            var rs = await _mediator.Send(new GetGamesQuery(pagingRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get game by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GameResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGame(int id)
        {
            var rs = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Get report of game
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy ="Admin")]
        [HttpGet("game-report")]
        [ProducesResponseType(typeof(GameReportResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReportgame()
        {
            var rs = await _mediator.Send(new GetReportOfGameQuery());
            return Ok(rs);
        }
      
    }
}
