using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Contests.Commands.CreateContest;
using ThinkTank.Application.CQRS.Contests.Commands.DeleteContest;
using ThinkTank.Application.CQRS.Contests.Commands.UpdateContest;
using ThinkTank.Application.CQRS.Contests.Queries.GetContestById;
using ThinkTank.Application.CQRS.Contests.Queries.GetContests;
using ThinkTank.Application.CQRS.Contests.Queries.GetLeaderboardOfContest;
using ThinkTank.Application.CQRS.Contests.Queries.GetReportOfContest;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ThinkTank.API.Controllers
{
    [Route("api/contests")]
    [ApiController]
    public class ContestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContestsController(IMediator mediator)
        {
            _mediator=mediator;
        }
        /// <summary>
        /// Get list of contest (1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="contestRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "All")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<ContestResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContests([FromQuery] PagingRequest pagingRequest, [FromQuery] ContestRequest contestRequest)
        {
            var rs = await _mediator.Send(new GetContestsQuery(pagingRequest,contestRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get lederboard of contest
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}/leaderboard")]
        [ProducesResponseType(typeof(PagedResults<ContestResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLeaderboardOfContest(int id, [FromQuery] PagingRequest request)
        {
            var rs = await _mediator.Send(new GetLeaderboardOfContestQuery(id, request));   
            return Ok(rs);
        }
        /// <summary>
        /// Get contest by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetContest(int id)
        {
            var rs = await _mediator.Send(new GetContestByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Create Contest
        /// </summary>
        /// <param name="contestRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateContest([FromBody] CreateAndUpdateContestRequest contestRequest)
        {
            var rs = await _mediator.Send(new CreateContestCommand(contestRequest));    
            return Ok(rs);
        }

        /// <summary>
        /// Update contest
        /// </summary>
        /// <param name="contestRequest"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateContest([FromBody] CreateAndUpdateContestRequest contestRequest, int id)
        {
            var rs = await _mediator.Send(new UpdateContestCommand(contestRequest, id));
            if (rs == null) return NotFound();
            return Ok(rs);
        }

        /// <summary>
        /// Delete contest
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteContest(int id)
        {
            var rs = await _mediator.Send(new DeleteContestCommand(id));
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Get Report Of Contest
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("contest-report")]
        [ProducesResponseType(typeof(ContestReportResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReportOfContest()
        {
            var rs = await _mediator.Send(new GetReportOfContestQuery());
            return Ok(rs);
        }
    }
}
