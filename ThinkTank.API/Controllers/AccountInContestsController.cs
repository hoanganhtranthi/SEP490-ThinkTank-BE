using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Contests.Commands.JoinContest;
using ThinkTank.Application.CQRS.Contests.Commands.UpdateAccountInContest;
using ThinkTank.Application.CQRS.Contests.Queries.GetAccountInContests;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountInContests")]
    [ApiController]
    public class AccountInContestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountInContestsController(IMediator mediator)
        {
            _mediator=mediator; 
        }

        /// <summary>
        /// Get list of account in contest 
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountInContestRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<AccountInContestResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccountInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountInContestRequest accountInContestRequest)
        {
            var rs = await _mediator.Send(new GetAccountInContestsQuery(pagingRequest,accountInContestRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Create Account In Contest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost()]
        [ProducesResponseType(typeof(AccountInContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAccountInContest([FromBody] CreateAccountInContestRequest request)
        {
            var rs = await _mediator.Send(new JoinContestCommand(request));
            return Ok(rs);
        }
       
        /// <summary>
        /// Update Account In Contest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpPut]
        [ProducesResponseType(typeof(AccountInContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAccountInContest([FromBody] UpdateAccountInContestRequest request)
        {
            var rs = await _mediator.Send(new UpdateAccountInContestCommand(request));
            return Ok(rs);
        }
    }
}
