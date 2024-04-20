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
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<RoomResponse>>> GetRooms([FromQuery] PagingRequest pagingRequest, [FromQuery] RoomRequest roomRequest)
        {
            var rs = await roomService.GetRooms(roomRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get room  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
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
      //  [Authorize(Policy = "Player")]
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
        [Authorize(Policy = "Player")]
        [HttpDelete("{id:int},{accountId:int}")]
        public async Task<ActionResult<RoomResponse>> DeleteRoom(int id, int accountId)
        {
            var rs = await roomService.DeleteRoom(id, accountId);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Get lederboard of party room of the game
        /// </summary>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{roomCode}/leaderboard")]
        public async Task<ActionResult<List<LeaderboardResponse>>> GetLeaderboard(string roomCode)
        {
            var rs = await roomService.GetLeaderboardOfRoom(roomCode);
            return Ok(rs);
        }
        /// <summary>
        /// Add Member To Room
        /// </summary>
        /// <param name="roomCode"></param>
        /// <param name="createAccountInRoomRequests"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{roomCode}")]
        public async Task<ActionResult<RoomResponse>> UpdateRoom(string roomCode, [FromBody] List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests)
        {
            var rs = await roomService.UpdateRoom(roomCode,createAccountInRoomRequests);
            return Ok(rs);
        }
        /// <summary>
        /// Member leave room
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{roomId:int},{accountId:int}")]
        public async Task<ActionResult<RoomResponse>> LeaveRoom(int roomId, int accountId)
        {
            var rs = await roomService.LeaveRoom(roomId, accountId);
            return Ok(rs);
        }
        /// <summary>
        /// Start Room To Play Game
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roomCode"></param>
        /// <param name="time"></param>
        /// <returns></returns>
       // [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{roomCode},{time:int}/started-room")]
        public async Task<ActionResult<RoomResponse>> GetToStartRoom(int accountId,string roomCode,int time)
        {
            var rs = await roomService.GetToStartRoom(roomCode,accountId,time);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
