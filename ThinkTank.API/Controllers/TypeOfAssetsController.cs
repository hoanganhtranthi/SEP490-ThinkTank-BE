using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Accounts.Queries.GetAccountById;
using ThinkTank.Application.CQRS.TypeOfAssets.Queries.GetTypeOfAssets;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/typeOfAssets")]
    [ApiController]
    public class TypeOfAssetsController : Controller
    {
        private readonly IMediator _mediator;
        public TypeOfAssetsController(IMediator mediator)
        {
           _mediator = mediator;
        }

        /// <summary>
        /// Get list type of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<TypeOfAssetResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTypeOfAssets([FromQuery] PagingRequest pagingRequest)
        {
            var rs = await _mediator.Send(new GetTypeOfAssetsQuery(pagingRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TypeOfAssetResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTypeOfAssetsById(int id)
        {
            var rs = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(rs);
        }

    }
}
