using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

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
        /// Get list of icons (StatusIconType: 1:All, 2:Icon the account has purchased, 3:icon that the account has not purchased)
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
    }
}
