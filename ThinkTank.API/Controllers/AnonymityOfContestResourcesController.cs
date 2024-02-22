using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/anonymityOfContestResources")]
    [ApiController]
    public class AnonymityOfContestResourcesController : ControllerBase
    {
        private readonly IAnonymousResourceOfContestService _anonymousResourceOfContestService;

        public AnonymityOfContestResourcesController(IAnonymousResourceOfContestService anonymousResourceOfContestService)
        {
            _anonymousResourceOfContestService = anonymousResourceOfContestService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AnonymityOfContestResponse>>> GetAnonymityOfContestResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceOfContestRequest anonymousRequest)
        {
            var rs = await _anonymousResourceOfContestService.GetAnonymityOfContestResources(anonymousRequest, pagingRequest);
            return Ok(rs);
        }
        
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnonymityOfContestResponse>> GetAnonymityOfContests(int id)
        {
            var rs = await _anonymousResourceOfContestService.GetAnonymityOfContestResourceById(id);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<AnonymityOfContestResponse>> CreateAnonymityOfContestResource([FromBody] AnonymityOfContestRequest resource)
        {
            var rs = await _anonymousResourceOfContestService.CreateAnonymityOfContestResource(resource);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AnonymityOfContestResponse>> UpdateAnonymityOfContestResource([FromBody] AnonymityOfContestRequest request, int id)
        {
            var rs = await _anonymousResourceOfContestService.UpdateAnonymityOfContestResource(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<AnonymityOfContestResponse>> DeleteAnonymityOfContestResource(int id)
        {
            var rs = await _anonymousResourceOfContestService.DeleteAnonymityOfContestResource(id);
            return Ok(rs);
        }
    }
}
