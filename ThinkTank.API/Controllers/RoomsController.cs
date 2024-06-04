using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Accounts.Queries.GetAccountById;
using ThinkTank.Application.CQRS.Rooms.Commands.CancelRoom;
using ThinkTank.Application.CQRS.Rooms.Commands.CreateRoom;
using ThinkTank.Application.CQRS.Rooms.Commands.LeaveRoom;
using ThinkTank.Application.CQRS.Rooms.Commands.RemoveRoomPartyInRealtimeDatabase;
using ThinkTank.Application.CQRS.Rooms.Commands.UpdateRoom;
using ThinkTank.Application.CQRS.Rooms.Queries.GetLeaderboardOfRoom;
using ThinkTank.Application.CQRS.Rooms.Queries.GetRooms;
using ThinkTank.Application.CQRS.Rooms.Queries.GetToStartRoom;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : Controller
    {
        private readonly IMediator _mediator;
        public RoomsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get list of rooms
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="roomRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<RoomResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRooms([FromQuery] PagingRequest pagingRequest, [FromQuery] RoomRequest roomRequest)
        {
            var rs = await _mediator.Send(new GetRoomsQuery(pagingRequest,roomRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get room  by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoomsById(int id)
        {
            var rs = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(rs);
        }
        /// <summary>
        /// Create room  
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost()]
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest room)
        {
            var rs = await _mediator.Send(new CreateRoomCommand(room));
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
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteRoom(int id, int accountId)
        {
            var rs = await _mediator.Send(new CancelRoomCommand(id, accountId));
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
        [ProducesResponseType(typeof(List<LeaderboardResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLeaderboard(string roomCode)
        {
            var rs = await _mediator.Send(new GetLeaderboardOfRoomQuery(roomCode));
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
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateRoom(string roomCode, [FromBody] List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests)
        {
            var rs = await _mediator.Send(new UpdateRoomCommand(roomCode,createAccountInRoomRequests));
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
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LeaveRoom(int roomId, int accountId)
        {
            var rs = await _mediator.Send(new LeaveRoomCommand(roomId, accountId));
            return Ok(rs);
        }
        /// <summary>
        /// Start Room To Play Game
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="roomCode"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{roomCode},{time:int}/started-room")]
        [ProducesResponseType(typeof(RoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToStartRoom(int accountId,string roomCode,int time)
        {
            var rs = await _mediator.Send(new GetToStartRoomQuery(roomCode,accountId,time));
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Remove Room Party In Real-time Database
        /// </summary>
        /// <param name="delayTime"></param>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{delayTime:int},{roomCode}/room-removed")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RemoveRoomPartyInRealtimeDatabase(string roomCode, int delayTime)
        {
            var rs = await _mediator.Send(new RemoveRoomPartyInRealtimeDatabaseCommand(roomCode, delayTime));
            return Ok(rs);
        }
    }
}
