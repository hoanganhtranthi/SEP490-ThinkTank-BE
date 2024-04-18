using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountInRooms")]
    [ApiController]
    public class AccountInRoomsController : Controller
    {
        private readonly IAccountInRoomService accountIn1Vs1Service;
        public AccountInRoomsController(IAccountInRoomService accountIn1Vs1Service)
        {
            this.accountIn1Vs1Service = accountIn1Vs1Service;
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
            var rs = await accountIn1Vs1Service.GetAccountInRooms(accountInRoomRequest, pagingRequest);
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
            var rs = await accountIn1Vs1Service.GetAccountInRoomById(id);
            return Ok(rs);
        }

        /// <summary>
        /// Create Account In Room
        /// </summary>
        /// <param name="request"></param>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{roomCode}")]
        public async Task<ActionResult<AccountInRoomResponse>> UpdateAccountInRoom(string roomCode,[FromBody] CreateAndUpdateAccountInRoomRequest request)
        {
            var rs = await accountIn1Vs1Service.UpdateAccountInRoom(roomCode,request);
            return Ok(rs);
        }
    }
}
