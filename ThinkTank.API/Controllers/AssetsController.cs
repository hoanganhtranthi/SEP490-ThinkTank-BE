using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Assets.Commands.CreateAsset;
using ThinkTank.Application.CQRS.Assets.Commands.DeleteAsset;
using ThinkTank.Application.CQRS.Assets.Queries.GetAssetById;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/assets")]
    [ApiController]
    public class AssetsController : Controller
    {
        private readonly IMediator _mediator;
        public AssetsController(IMediator mediator)
        {
            _mediator=mediator;
        }

        /// <summary>
        /// Get list of assets
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "All")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<AssetResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssets([FromQuery] PagingRequest pagingRequest, [FromQuery] AssetRequest assetRequest)
        {
            var rs = await _mediator.Send(GetAssets(pagingRequest,assetRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get type of asset  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AssetResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssetsById(int id)
        {
            var rs = await _mediator.Send(new GetAssetByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Create asset  
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
     [Authorize(Policy = "Admin")]
        [HttpPost()]
        [ProducesResponseType(typeof(AssetResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAsset([FromBody] List<CreateAssetRequest> resource)
        {
            var rs = await _mediator.Send(new CreateAssetCommand(resource));
            return Ok(rs);
        }
        /// <summary>
        /// Delete asset
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [ProducesResponseType(typeof(AssetResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAsset([FromBody] List<int> assetId)
        {
            var rs = await _mediator.Send(new DeleteAssetCommand(assetId));
            return Ok(rs);
        }
    }
}
