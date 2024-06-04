using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Friends.Commands.AddFriend;
using ThinkTank.Application.CQRS.Friends.Commands.UnFriend;
using ThinkTank.Application.CQRS.Friends.Queries.GetFriendById;
using ThinkTank.Application.CQRS.Friends.Queries.GetFriends;
using ThinkTank.Application.CQRS.Friends.UpdateStatusFriendship;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {

        private readonly IMediator _mediator;
        public FriendsController(IMediator mediator)
        {
            _mediator=mediator;
        }
        /// <summary>
        /// Get list of friendships of account Id (1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="friendRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet]
        [ProducesResponseType(typeof(FriendResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetFriends([FromQuery] PagingRequest pagingRequest, [FromQuery] FriendRequest friendRequest)
        {
            var rs = await _mediator.Send(new GetFriendsQuery(pagingRequest,friendRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get friendship by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(FriendResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetFriend(int id)
        {
            var rs = await _mediator.Send(new GetFriendByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Accept friend
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
         [Authorize(Policy = "Player")]
        [HttpGet("{friendId:int}/status")]
        [ProducesResponseType(typeof(FriendResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToUpdateStatus(int friendId)
        {
            var rs = await _mediator.Send(new UpdateStatusFriendshipCommand(friendId));
            return Ok(rs);
        }
        /// <summary>
        /// Add friend
        /// </summary>
        /// <param name="friend"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost()]
        [ProducesResponseType(typeof(FriendResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddFriend([FromBody] CreateFriendRequest friend)
        {
            var rs = await _mediator.Send(new AddFriendCommand(friend));    
            return Ok(rs);
        }
        /// <summary>
        /// Unaccept friend
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(FriendResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteFriendship(int id)
        {
            var rs = await _mediator.Send(new UnFriendCommand(id));
            return Ok(rs);
        }
    }
}
