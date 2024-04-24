using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers

{
    [Route("api/topics")]
    [ApiController]
    public class TopicsController : Controller
    {
        
        private readonly ITopicService _topicService;
        public TopicsController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        /// <summary>
        /// Get list of topic (StatusTopicType: 1: All, 2: Has Asset, 3: No Asset)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy ="All")]
        [HttpGet]
        public async Task<ActionResult<List<TopicResponse>>> GetTopics([FromQuery] PagingRequest pagingRequest, [FromQuery] TopicRequest request)
        {
            var rs = await _topicService.GetTopics(request, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get topic by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy ="All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TopicResponse>> GetTopic(int id)
        {
            var rs = await _topicService.GetTopicById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create Topic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
       [HttpPost()]
        public async Task<ActionResult<TopicResponse>> CreatTopic([FromBody] CreateTopicRequest request)
        {
            var rs = await _topicService.CreateTopic(request);
            return Ok(rs);
        }
    }
}
