using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/flipCardAndImagesWalkthroughResources")]
    [ApiController]
    public class FlipCardAndImagesWalkthroughResourcesController : Controller
    {
       private readonly IFlipCardAndImagesWalkthroughResourceService _flipCardAndImagesWalkthroughResourcesService;
        public FlipCardAndImagesWalkthroughResourcesController(IFlipCardAndImagesWalkthroughResourceService flipCardAndImagesWalkthroughResourcesService)
        {
            _flipCardAndImagesWalkthroughResourcesService = flipCardAndImagesWalkthroughResourcesService;
        }

        /// <summary>
        /// Get list of flipcard and images walkthrough  resources
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="musicPasswordRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<FlipCardAndImagesWalkthroughResponse>>> GetFlipCardAndImagesWalkthroughResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceRequest musicPasswordRequest)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesService.GetFlipCardAndImagesWalkthroughResources(musicPasswordRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get flipcard and images walkthrough resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughResponse>> GetFlipCardAndImagesWalkthrough(int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesService.GetFlipCardAndImagesWalkthroughResourceById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create flipcard and images walkthrough resource 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughResponse>> CreateFlipCardAndImagesWalkthroughResource([FromBody] FlipCardAndImagesWalkthroughRequest resource)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesService.CreateFlipCardAndImagesWalkthroughResource(resource);
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
        public async Task<ActionResult<FlipCardAndImagesWalkthroughResponse>> UpdateResource([FromBody] FlipCardAndImagesWalkthroughRequest request, int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesService.UpdateFlipCardAndImagesWalkthroughResource(id, request);
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
        public async Task<ActionResult<FlipCardAndImagesWalkthroughResponse>> DeleteResource(int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesService.DeleteFlipCardAndImagesWalkthroughResource(id);
            return Ok(rs);
        }
    }
}
