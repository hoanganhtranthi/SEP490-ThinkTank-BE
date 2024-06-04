using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Icons.Commands.BuyIcon;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/iconOfAccounts")]
    [ApiController]
    public class IconOfAccountsController : Controller
    {
        private readonly IMediator _mediator;
        public IconOfAccountsController(IMediator mediator)
        {
            _mediator=mediator;
        }
        /// <summary>
        /// Buy Icon
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost]
        [ProducesResponseType(typeof(IconOfAccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateIconOfAccount([FromBody] CreateIconOfAccountRequest request)
        {
            var rs = await _mediator.Send(new BuyIconCommand(request));
            return Ok(rs);
        }
    }
}
