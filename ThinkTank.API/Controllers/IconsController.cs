using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/icons")]
    [ApiController]
    public class IconsController : Controller
    {
        private readonly IIconService _iconService;
        public IconsController(IIconService iconService)
        {
            _iconService = iconService;
        }

        /// <summary>
        /// Get list of icons
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="iconRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet]
        public async Task<ActionResult<List<IconResponse>>> GetIcons([FromQuery] PagingRequest pagingRequest, [FromQuery] IconRequest iconRequest)
        {
            var rs = await _iconService.GetIcons(iconRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get icon by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IconResponse>> GetIcon(int id)
        {
            var rs = await _iconService.GetIconById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Update status of icon
        /// </summary>
        /// <param name="iconId"></param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [HttpGet("{iconId:int}/status")]
        public async Task<ActionResult<IconResponse>> GetToUpdateStatus(int iconId)
        {
            var rs = await _iconService.GetToUpdateStatus(iconId);
            return Ok(rs);
        }
        /// <summary>
        /// Add icon
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
       // [Authorize(Policy = "")]
        [HttpPost()]
        public async Task<ActionResult<IconResponse>> AddIcon([FromBody] CreateIconRequest icon)
        {
            var rs = await _iconService.CreateIcon(icon);
            return Ok(rs);
        }
        /// <summary>
        /// Buy Icon 
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpPost("iconOfAccount")]
        public async Task<ActionResult<IconResponse>> AddIconOfAccount([FromBody] IconOfAccountRequest icon)
        {
            var rs = await _iconService.CreateIconOfAccount(icon);
            return Ok(rs);
        }
    }
}
