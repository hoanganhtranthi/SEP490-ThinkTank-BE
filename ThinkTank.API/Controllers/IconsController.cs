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
        [Authorize(Policy = "Player")]
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
        [Authorize(Policy = "Player")]
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
       //[Authorize(Policy = "")]
        [HttpGet("{iconId:int}/status")]
        public async Task<ActionResult<IconResponse>> GetToUpdateStatus(int iconId)
        {
            var rs = await _iconService.GetToUpdateStatus(iconId);
            return Ok(rs);
        }
    }
}
