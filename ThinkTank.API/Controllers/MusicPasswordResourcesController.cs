using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/musicPasswordResources")]
    [ApiController]
    public class MusicPasswordResourcesController : Controller
    {
        private readonly IMusicPasswordResourceService _musicPasswordResourceService;
        public MusicPasswordResourcesController(IMusicPasswordResourceService musicPasswordResourceService)
        {
            _musicPasswordResourceService = musicPasswordResourceService;
        }
        /// <summary>
        /// Get list of musicPassword resources
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="musicPasswordRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<MusicPasswordResponse>>> GetMusicPasswordResources([FromQuery] PagingRequest pagingRequest, [FromQuery] ResourceRequest musicPasswordRequest)
        {
            var rs = await _musicPasswordResourceService.GetMusicPasswordResources(musicPasswordRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get musicPassword resource by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MusicPasswordResponse>> GetMusicPassword(int id)
        {
            var rs = await _musicPasswordResourceService.GetMusicPasswordResourceById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create MusicPassword Resource 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpPost()]
        public async Task<ActionResult<MusicPasswordResponse>> CreateMusicPasswordResource([FromBody] MusicPasswordRequest resource)
        {
            var rs = await _musicPasswordResourceService.CreateMusicPasswordResource(resource);
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
        public async Task<ActionResult<MusicPasswordResponse>> UpdateResource([FromBody] MusicPasswordRequest request, int id)
        {
            var rs = await _musicPasswordResourceService.UpdateMusicPasswordResource(id, request);
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
        public async Task<ActionResult<MusicPasswordResponse>> DeleteResource(int id)
        {
            var rs = await _musicPasswordResourceService.DeleteMusicPasswordResource(id);
            return Ok(rs);
        }

    }
}
