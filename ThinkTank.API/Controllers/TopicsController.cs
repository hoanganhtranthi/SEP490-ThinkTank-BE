using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Accounts.Queries.GetAccountById;
using ThinkTank.Application.CQRS.Topics.Commands.CreateTopic;
using ThinkTank.Application.CQRS.Topics.Queries.GetTopics;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers

{
    [Route("api/topics")]
    [ApiController]
    public class TopicsController : Controller
    {
        
        private readonly IMediator _mediator;
        public TopicsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get list of topic (StatusTopicType: 1: All, 2: Has Asset, 3: No Asset)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy ="All")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<TopicResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTopics([FromQuery] PagingRequest pagingRequest, [FromQuery] TopicRequest request)
        {
            var rs = await _mediator.Send(new GetTopicsQuery(pagingRequest,request));
            return Ok(rs);
        }
        /// <summary>
        /// Get topic by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy ="All")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TopicResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTopic(int id)
        {
            var rs = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Create Topic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
       [HttpPost()]
        [ProducesResponseType(typeof(TopicResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreatTopic([FromBody] CreateTopicRequest request)
        {
            var rs = await _mediator.Send(new CreateTopicCommand(request));
            return Ok(rs);
        }
    }
}
