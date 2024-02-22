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

        // GET: api/<ContestController>
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<ContestResponse>>> GetContests([FromQuery] PagingRequest pagingRequest, [FromQuery] CreateContestRequest contestRequest)
        {
            var rs = await _contestService.GetContests(contestRequest, pagingRequest);
            return Ok(rs);
        }

        // GET api/<ContestController>/5
        //[Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ContestResponse>> GetContest(int id)
        {
            var rs = await _contestService.GetContestById(id);
            return Ok(rs);
        }

        // POST api/<ContestController>
        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ContestResponse>> CreateContest([FromBody] CreateContestRequest contestRequest)
        {
            var rs = await _contestService.CreateContest(contestRequest);
            return Ok(rs);
        }

        // PUT api/<ContestController>/5
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ContestResponse>> UpdateContest([FromBody] CreateContestRequest contestRequest, int id)
        {
            var rs = await _contestService.UpdateContest(id, contestRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }

        // DELETE api/<ContestController>/5
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
