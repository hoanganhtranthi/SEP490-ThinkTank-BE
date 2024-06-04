using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Rooms.Commands.UpdateAccountInRoom;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountInRooms")]
    [ApiController]
    public class AccountInRoomsController : Controller
    {
        private readonly IMediator _mediator;
        public AccountInRoomsController(IMediator mediator)
        {
            _mediator = mediator;  
        }
        /// <summary>
        /// Update result of account In Room
        /// </summary>
        /// <param name="request"></param>
        /// <param name="roomCode"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{roomCode}")]
        [ProducesResponseType(typeof(AccountInRoomResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAccountInRoom(string roomCode,[FromBody] CreateAndUpdateAccountInRoomRequest request)
        {
            var rs = await _mediator.Send(new UpdateAccountInRoomCommand(roomCode, request));
            return Ok(rs);
        }
    }
}
