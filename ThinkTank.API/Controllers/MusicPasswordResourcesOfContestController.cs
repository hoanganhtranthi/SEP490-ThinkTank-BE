using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/musicPasswordResourcesOfContest")]
    [ApiController]
    public class MusicPasswordResourcesOfContestController : ControllerBase
    {
        private readonly IMusicPasswordResourceOfContestService _musicPasswordResourceOfContestService;
        public MusicPasswordResourcesOfContestController(IMusicPasswordResourceOfContestService musicPasswordResourceOfContestService)
        {
            _musicPasswordResourceOfContestService = musicPasswordResourceOfContestService;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<MusicPasswordOfContestResponse>>> GetMusicPasswordResourcesOfContests([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceOfContestRequest musicPasswordRequest)
        {
            var rs = await _musicPasswordResourceOfContestService.GetMusicPasswordResourcesOfContest(musicPasswordRequest, pagingRequest);
            return Ok(rs);
        }
        
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MusicPasswordOfContestResponse>> GetMusicPasswordResourceOfContest(int id)
        {
            var rs = await _musicPasswordResourceOfContestService.GetMusicPasswordResourceOfContestById(id);
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<MusicPasswordOfContestResponse>> CreateMusicPasswordResourcesOfContest([FromBody] MusicPasswordOfContestRequest resource)
        {
            var rs = await _musicPasswordResourceOfContestService.CreateMusicPasswordResourceOfContest(resource);
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<MusicPasswordOfContestResponse>> UpdateMusicPasswordResourcesOfContest([FromBody] MusicPasswordOfContestRequest request, int id)
        {
            var rs = await _musicPasswordResourceOfContestService.UpdateMusicPasswordResourceOfContest(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<MusicPasswordOfContestResponse>> DeleteMusicPasswordResourcesOfContest(int id)
        {
            var rs = await _musicPasswordResourceOfContestService.DeleteMusicPasswordResourceOfContest(id);
            return Ok(rs);
        }
    }
}
