using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {

        private readonly IFriendService _friendService;
        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
        }
        /// <summary>
        /// Get list of friendships of account Id (1: All, 2 : True, 3:False, 4: Null)
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="friendRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet]
        public async Task<ActionResult<List<FriendResponse>>> GetFriends([FromQuery] PagingRequest pagingRequest, [FromQuery] FriendRequest friendRequest)
        {
            var rs = await _friendService.GetFriends(friendRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get friendship by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FriendResponse>> GetFriend(int id)
        {
            var rs = await _friendService.GetFriendById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Accept friend
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
         [Authorize(Policy = "Player")]
        [HttpGet("{friendId:int}/status")]
        public async Task<ActionResult<FriendResponse>> GetToUpdateStatus(int friendId)
        {
            var rs = await _friendService.GetToUpdateStatus(friendId);
            return Ok(rs);
        }
        /// <summary>
        /// Add friend
        /// </summary>
        /// <param name="friend"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpPost()]
        public async Task<ActionResult<FriendResponse>> AddFriend([FromBody] CreateFriendRequest friend)
        {
            var rs = await _friendService.CreateFriend(friend);
            return Ok(rs);
        }
        /// <summary>
        /// Unaccept friend
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<FriendResponse>> DeleteFriendship(int id)
        {
            var rs = await _friendService.DeleteFriendship(id);
            return Ok(rs);
        }
    }
}
