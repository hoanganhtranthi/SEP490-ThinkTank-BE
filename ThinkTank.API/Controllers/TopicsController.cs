using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers

{
    [Route("api/topics")]
    [ApiController]
    public class TopicsController : Controller
    {
        /*
        private readonly ITopicService _topicService;
        public TopicsController(ITopicService topicService)
        {
            _topicService = topicService;
        }
        /// <summary>
        /// Get list of topic
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="anonymousRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
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
        [AllowAnonymous]
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
       // [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<TopicResponse>> CreateAnonymousResource([FromBody] CreateTopicOfGameRequest request)
        {
            var rs = await _topicService.CreateTopic(request);
            return Ok(rs);
        }
        /// <summary>
        /// Update topic
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TopicResponse>> UpdateResource([FromBody] string name, int id)
        {
            var rs = await _topicService.UpdateTopic(id, name);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        */
    }
}
