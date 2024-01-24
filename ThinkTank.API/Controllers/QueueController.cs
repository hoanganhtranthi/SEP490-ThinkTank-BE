using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/queues")]
    [ApiController]
    public class QueueController : Controller
    {
        private readonly IQueueService _queueService;
        public QueueController(IQueueService queueService)
        {
            _queueService = queueService;
        }
        [HttpPost("join-matchmaking")]
        public async Task<IActionResult> JoinMatchmaking(string playerId)
        {
            try
            {
                await _queueService.EnqueuePlayer(playerId);
                return Ok(new { Message = "Joined matchmaking queue successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error: {ex.Message}" });
            }
        }
        [Benchmark]
        [HttpPost("matchmake")]
        public async Task<IActionResult> MatchmakePlayers(string playerId)
        {
            try
            {
                var matchedPlayers = await _queueService.GetRandomPlayersFromQueue(playerId);
                if (matchedPlayers != null)
                {
                    return Ok(new { MatchedPlayers = matchedPlayers });
                }
                else
                {
                    return Ok(new { Message = "Not enough players in the queue." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error: {ex.Message}" });
            }
        }
    }
}
