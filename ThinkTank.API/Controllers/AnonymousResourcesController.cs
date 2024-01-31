using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/anonymousResources")]
    [ApiController]
    public class AnonymousResourcesController : Controller
    {
       private readonly IAnonymousResourceService _anonymousResourceService;

        public AnonymousResourcesController(IAnonymousResourceService anonymousResourceService)
        {
            _anonymousResourceService = anonymousResourceService;
        }
        /// <summary>
        /// Get list of anonymous resources
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="anonymousRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AnonymousResponse>>> GetAnonymousResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceRequest anonymousRequest)
        {
            var rs = await _anonymousResourceService.GetAnonymousResources(anonymousRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get anonymous resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnonymousResponse>> GetAnonymous(int id)
        {
            var rs = await _anonymousResourceService.GetAnonymousResourceById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create Anonymous Resource 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<AnonymousResponse>> CreateAnonymousResource([FromBody] AnonymousRequest resource)
        {
            var rs = await _anonymousResourceService.CreateAnonymousResource(resource);
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
        public async Task<ActionResult<AnonymousResponse>> UpdateResource([FromBody] AnonymousRequest request, int id)
        {
            var rs = await _anonymousResourceService.UpdateAnonymousResource(id,request);
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
        public async Task<ActionResult<AnonymousResponse>> DeleteResource(int id)
        {
            var rs = await _anonymousResourceService.DeleteAnonymousResource(id);
            return Ok(rs);
        }
    }
}
