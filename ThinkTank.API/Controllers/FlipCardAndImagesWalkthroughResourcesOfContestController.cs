using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/flipCardAndImagesWalkthroughResourcesOfContest")]
    [ApiController]
    public class FlipCardAndImagesWalkthroughResourcesOfContestController : ControllerBase
    {
        private readonly IFlipCardAndImagesWalkthroughResourceOfContestService _flipCardAndImagesWalkthroughResourcesOfContestService;
        public FlipCardAndImagesWalkthroughResourcesOfContestController(IFlipCardAndImagesWalkthroughResourceOfContestService flipCardAndImagesWalkthroughResourcesOfContestService)
        {
            _flipCardAndImagesWalkthroughResourcesOfContestService = flipCardAndImagesWalkthroughResourcesOfContestService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<FlipCardAndImagesWalkthroughOfContestResponse>>> GetFlipCardAndImagesWalkthroughOfContestResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceOfContestRequest flipCardAndImagesWalkthroughOfContestRequest)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesOfContestService.GetFlipCardAndImagesWalkthroughResourcesOfContest(flipCardAndImagesWalkthroughOfContestRequest, pagingRequest);
            return Ok(rs);
        }
        
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughOfContestResponse>> GetFlipCardAndImagesWalkthroughOfContestResources(int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesOfContestService.GetFlipCardAndImagesWalkthroughResourceOfContestById(id);
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughOfContestResponse>> CreateFlipCardAndImagesWalkthroughOfContestResource([FromBody] FlipCardAndImagesWalkthroughOfContestRequest resource)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesOfContestService.CreateFlipCardAndImagesWalkthroughResourceOfContest(resource);
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughOfContestResponse>> UpdateFlipCardAndImagesWalkthroughOfContestResource([FromBody] FlipCardAndImagesWalkthroughOfContestRequest request, int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesOfContestService.UpdateFlipCardAndImagesWalkthroughResourceOfContest(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<FlipCardAndImagesWalkthroughOfContestResponse>> DeleteFlipCardAndImagesWalkthroughOfContestResource(int id)
        {
            var rs = await _flipCardAndImagesWalkthroughResourcesOfContestService.DeleteFlipCardAndImagesWalkthroughResourceOfContest(id);
            return Ok(rs);
        }
    }
}