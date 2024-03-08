using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/achievements")]
    [ApiController]
    public class AchieviementsController : Controller
    {
        private readonly IAchievementService _achievementService;
        public AchieviementsController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }
        /// <summary>
        /// Get list of achievements
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="request"></param>
        /// <returns></returns>
       // [Authorize(Policy ="All")]
        [HttpGet]
        public async Task<ActionResult<List<AchievementResponse>>> GetAchievements([FromQuery] PagingRequest pagingRequest, [FromQuery] AchievementRequest request)
        {
            var rs = await _achievementService.GetAchievements(request, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get achievement  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Policy ="All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AchievementResponse>> GetAchievement(int id)
        {
            var rs = await _achievementService.GetAchievementById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Add achievement
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       // [Authorize(Policy = "Player")]
        [HttpPost()]
        public async Task<ActionResult<AchievementResponse>> CreateAchievement([FromBody] CreateAchievementRequest request)
        {
            var rs = await _achievementService.CreateAchievement(request);
            return Ok(rs);
        }
        /// <summary>
        /// Get lederboard of a level of the game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
       // [Authorize(Policy = "All")]
        [HttpGet("{gameId:int}/leaderboard")]
        public async Task<ActionResult<List<ContestResponse>>> GetLeaderboard(int gameId)
        {
            var rs = await _achievementService.GetLeaderboard(gameId);
            return Ok(rs);
        }
    }
}
