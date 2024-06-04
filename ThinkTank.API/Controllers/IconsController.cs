using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Icons.Queries.GetIcons;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/icons")]
    [ApiController]
    public class IconsController : Controller
    {
        private readonly IMediator _mediator;
        public IconsController(IMediator mediator)
        {
            _mediator= mediator;
        }

        /// <summary>
        /// Get list of icons (StatusIconType: 1:All, 2:Icon the account has purchased, 3:icon that the account has not purchased)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="iconRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<IconResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIcons([FromQuery] PagingRequest pagingRequest, [FromQuery] IconRequest iconRequest)
        {
            var rs = await _mediator.Send(new GetIconsQuery(pagingRequest,iconRequest));
            return Ok(rs);
        }
    }
}
