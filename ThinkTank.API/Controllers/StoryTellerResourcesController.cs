using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/storyTellerResources")]
    [ApiController]
    public class StoryTellerResourcesController : Controller
    {
        private readonly IStoryTellerResourceService _storyTellerResourceService;
        public StoryTellerResourcesController(IStoryTellerResourceService storyTellerResourceService)
        {
            _storyTellerResourceService = storyTellerResourceService;
        }
        /// <summary>
        /// Get list of story teller  resources
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="musicPasswordRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<StoryTellerResponse>>> GetStoryTellerResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceRequest musicPasswordRequest)
        {
            var rs = await _storyTellerResourceService.GetStoryTellerResources(musicPasswordRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get story teller resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StoryTellerResponse>> GetStoryTeller(int id)
        {
            var rs = await _storyTellerResourceService.GetStoryTellerResourceById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create story teller resource 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<StoryTellerResponse>> CreateStoryTellerResource([FromBody] StoryTellerRequest resource)
        {
            var rs = await _storyTellerResourceService.CreateStoryTellerResource(resource);
            return Ok(rs);
        }
        /// <summary>
        /// Update resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<StoryTellerResponse>> UpdateResource([FromBody] StoryTellerRequest request, int id)
        {
            var rs = await _storyTellerResourceService.UpdateStoryTellerResource(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Delete resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<StoryTellerResponse>> DeleteResource(int id)
        {
            var rs = await _storyTellerResourceService.DeleteStoryTellerResource(id);
            return Ok(rs);
        }
    }
}
