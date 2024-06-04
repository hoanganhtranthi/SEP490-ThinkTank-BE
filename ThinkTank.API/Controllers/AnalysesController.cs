using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountId;
using ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountIdAndGameId;
using ThinkTank.Application.Analysis.Queries.GetAnalysisOfMemoryTypeByAccountId;
using ThinkTank.Application.Analysis.Queries.GetAverageScoreAnalysis;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/analyses")]
    [ApiController]
    public class AnalysesController : Controller
    {
        private readonly IMediator _mediator;
        public AnalysesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get analysis of account by account Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(AdminDashboardResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAnalysisOfAccount(int accountId)
        {
            var rs = await _mediator.Send(new GetAnalysisOfAccountIdQuery(accountId));
            return Ok(rs);
        }
        /// <summary>
        /// Get analysis of account by account Id and game Id
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet()]
        [ProducesResponseType(typeof(List<RatioMemorizedDailyResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAnalysisOfAccount([FromQuery] AnalysisRequest request)
        {
            var rs = await _mediator.Send(new GetAnalysisOfAccountIdAndGameIdQuery(request));
            return Ok(rs);
        }
        /// <summary>
        /// Get analysis of each type of account's memory by account Id 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int}/by-memory-type")]
        [ProducesResponseType(typeof(AnalysisOfMemoryTypeResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAnalysisOfEachTypeOfMemoryOfAccount(int accountId)
        {
            var rs = await _mediator.Send(new GetAnalysisOfMemoryTypeByAccountIdQuery(accountId));
            return Ok(rs);
        }
        /// <summary>
        /// Get analysis average score by account Id 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{userId:int},{gameId:int}/average-score")]
        [ProducesResponseType(typeof(AnalysisAverageScoreResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAverageScoreAnalysis(int gameId, int userId)
        {
            var rs = await _mediator.Send(new GetAverageScoreAnalysisQuery(userId,gameId));
            return Ok(rs);
        }
    }
}
