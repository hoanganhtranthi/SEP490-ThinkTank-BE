using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContestById;
using ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContests;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/typeOfAssetInContests")]
    [ApiController]
    public class TypeOfAssetInContestsController : Controller
    {
        private readonly IMediator _mediator;
        public TypeOfAssetInContestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get list of type assets in contest
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="contestId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<TypeOfAssetInContestResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTypeOfAssetInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] int? contestId)
        {
            var rs = await _mediator.Send(new GetTypeOfAssetInContestsQuery(pagingRequest, contestId));
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset in contest  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TypeOfAssetInContestResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTypeOfAssetsInContestById(int id)
        {
            var rs = await _mediator.Send(new GetTypeOfAssetsInContestByIdQuery(id));
            return Ok(rs);
        }
    }
}
