using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : Controller
    {
       private readonly IRoomService roomService;
        public RoomsController(IRoomService roomService)
        {
            this.roomService = roomService;
        }
        /// <summary>
        /// Get list of rooms
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="roomRequest"></param>
        /// <returns></returns>
       // [Authorize(Policy = "All")]
        [HttpGet]
        public async Task<ActionResult<List<RoomResponse>>> GetRooms([FromQuery] PagingRequest pagingRequest, [FromQuery] RoomRequest roomRequest)
        {
            var rs = await roomService.GetRooms(roomRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get type of room  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoomResponse>> GetRoomsById(int id)
        {
            var rs = await roomService.GetRoomById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create room  
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
       // [Authorize(Policy = "Player")]
        [HttpPost()]
        public async Task<ActionResult<RoomResponse>> CreateRoom([FromBody] CreateRoomRequest room)
        {
            var rs = await roomService.CreateRoom(room);
            return Ok(rs);
        }
        /// <summary>
        /// Cancel Room Party
        /// </summary>
        /// <param name="id"></param>
        ///  <param name="accountId"></param>
        /// <returns></returns>
       // [Authorize(Policy = "Player")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<RoomResponse>> DeleteRoom(int id, int accountId)
        {
            var rs = await roomService.DeleteRoom(id,accountId);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Get lederboard of party room of the game
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
       // [Authorize(Policy = "All")]
        [HttpGet("{roomId:int}/leaderboard")]
        public async Task<ActionResult<List<LeaderboardResponse>>> GetLeaderboard(int roomId)
        {
            var rs = await roomService.GetLeaderboardOfRoom(roomId);
            return Ok(rs);
        }
        /// <summary>
        /// Update Room
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
      //  [Authorize(Policy = "Player")]
        [HttpPut("{roomId:int}")]
        public async Task<ActionResult<List<LeaderboardResponse>>> UpdateRoom(int roomId, [FromBody] List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests)
        {
            var rs = await roomService.UpdateRoom(roomId,createAccountInRoomRequests);
            return Ok(rs);
        }
        /// <summary>
        /// Update status of room
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // [Authorize(Policy = "Player")]
        [HttpGet("{id}/ended-room")]
        public async Task<ActionResult<RoomResponse>> GetToUpdateStatusRoom(int id)
        {
            var rs = await roomService.GetToUpdateStatusRoom(id);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
