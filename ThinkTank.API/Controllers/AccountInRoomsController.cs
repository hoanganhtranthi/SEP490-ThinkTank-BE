using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountInRooms")]
    [ApiController]
    public class AccountInRoomsController : Controller
    {
        private readonly IAccountInRoomService _accountInRoomService;
        public AccountInRoomsController(IAccountInRoomService accountInRoomService)
        {
            _accountInRoomService = accountInRoomService;   
        }
        /// <summary>
        /// Get list of account in room
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountInRoomRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AccountInRoomResponse>>> GetAccountInRooms([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountInRoomRequest accountInRoomRequest)
        {
            var rs = await _accountInRoomService.GetAccountInRooms(accountInRoomRequest, pagingRequest);
            return Ok(rs);
        }

        /// <summary>
        /// Get account in room by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountInRoomResponse>> GetAccountInRoomById(int id)
        {
            var rs = await _accountInRoomService.GetAccountInRoomById(id);
            return Ok(rs);
        }

        /// <summary>
        /// Update result of account In Room
        /// </summary>
        /// <param name="request"></param>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{roomCode}")]
        public async Task<ActionResult<AccountInRoomResponse>> UpdateAccountInRoom(string roomCode,[FromBody] CreateAndUpdateAccountInRoomRequest request)
        {
            var rs = await _accountInRoomService.UpdateAccountInRoom(roomCode,request);
            return Ok(rs);
        }
    }
}
