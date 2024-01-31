using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GamesController : Controller
    {
       private readonly IGameService _gameService;
        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }
        /// <summary>
        /// Get list of games
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="gameRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GameResponse>>> GetGames([FromQuery] PagingRequest pagingRequest, [FromQuery] GameRequest gameRequest)
        {
            var rs = await _gameService.GetGames(gameRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get game by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GameResponse>> GetGame(int id)
        {
            var rs = await _gameService.GetGameById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Get list game level by  game Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pagingRequest"></param>
        /// <returns></returns>
        //[Authorize(Policy ="Admin")]
        [HttpGet("{id:int}/game-level")]
        public async Task<ActionResult<List<GameResponse>>> GetGameLevel(int id, [FromQuery] PagingRequest pagingRequest)
        {
            var rs = await _gameService.GetGameLevelById(id,pagingRequest);
            return Ok(rs);
        }

    }
}
