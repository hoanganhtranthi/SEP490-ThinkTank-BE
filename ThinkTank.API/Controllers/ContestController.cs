using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ThinkTank.API.Controllers
{
    [Route("api/contests")]
    [ApiController]
    public class ContestController : ControllerBase
    {
        private readonly 
            IContestService _contestService;

        public ContestController(IContestService contestService)
        {
            _contestService = contestService;
        }
        /// <summary>
        /// Get list of contest (1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="contestRequest"></param>
        /// <returns></returns>
       // [Authorize(Policy = "All")]
        [HttpGet]
        public async Task<ActionResult<List<ContestResponse>>> GetContests([FromQuery] PagingRequest pagingRequest, [FromQuery] ContestRequest contestRequest)
        {
            var rs = await _contestService.GetContests(contestRequest, pagingRequest);
            return Ok(rs);
        }

        [HttpGet("contests-not-asset")]
        public async Task<ActionResult<List<ContestResponse>>> GetContestsNotAsset([FromQuery] PagingRequest pagingRequest, [FromQuery] ContestRequest contestRequest)
        {
            var rs = await _contestService.GetContestsNotAsset(contestRequest, pagingRequest);
            return Ok(rs);
        }

        /// <summary>
        /// Get lederboard of contest
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}/leaderboard")]
        public async Task<ActionResult<List<ContestResponse>>> GetLeaderboardOfContest(int id)
        {
            var rs = await _contestService.GetLeaderboardOfContest(id);
            return Ok(rs);
        }
        /// <summary>
        /// Get contest by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<ContestController>/5
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ContestResponse>> GetContest(int id)
        {
            var rs = await _contestService.GetContestById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create Contest
        /// </summary>
        /// <param name="contestRequest"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ContestResponse>> CreateContest([FromBody] CreateContestRequest contestRequest)
        {
            var rs = await _contestService.CreateContest(contestRequest);
            return Ok(rs);
        }

        /// <summary>
        /// Update contest
        /// </summary>
        /// <param name="contestRequest"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ContestResponse>> UpdateContest([FromBody] UpdateContestRequest contestRequest, int id)
        {
            var rs = await _contestService.UpdateContest(id, contestRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }

        /// <summary>
        /// Delete contest
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteContest(int id)
        {
            var rs = await _contestService.DeleteContest(id);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
