using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateAccountIn1vs1;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateRoomPlayCountervailingWithFriend;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.FindAccountTo1vs1;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.RemoveRoom1vs1InRealtimeDatabase;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.StartRoomIn1vs1;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.UpdateAccount1vs1;
using ThinkTank.Application.CQRS.AccountIn1vs1s.Queries.RemoveAccountFromQueue;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountIn1vs1s")]
    [ApiController]
    public class AccountIn1vs1sController : Controller
    {
       private readonly IMediator _mediator;
        public AccountIn1vs1sController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create Account In 1vs1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost]
        [ProducesResponseType(typeof(AccountIn1vs1Response), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAccountIn1vs1([FromBody] CreateAndUpdateAccountIn1vs1Request request)
        {
            var rs = await _mediator.Send(new CreateAccountIn1vs1Command(request));
            return Ok(rs);
        }
        /// <summary>
        /// Update Account In 1vs1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut]
        [ProducesResponseType(typeof(AccountIn1vs1Response), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAccountIn1vs1([FromBody] CreateAndUpdateAccountIn1vs1Request request)
        {
            var rs = await _mediator.Send(new UpdateAccount1vs1Command(request));
            return Ok(rs);
        }
        /// <summary>
        /// Find Opponent of Account In 1vs1
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="gameId"></param>
        /// <param name="coin"></param>
        ///<returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{gameId:int},{coin:int}/opponent-of-account")]
        [ProducesResponseType(typeof(RoomIn1vs1Response), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FindAccountIn1vs1( int accountId,  int gameId,  int coin)
        {
            var rs = await _mediator.Send(new FindAccountTo1vs1Command(gameId, accountId, coin));
            return Ok(rs);
        }
        /// <summary>
        /// Match play countervailing mode with friend
        /// </summary>
        /// <param name="accountId1"></param>
        ///  <param name="gameId"></param>
        ///   <param name="accountId2"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpGet("{accountId1:int},{gameId:int},{accountId2:int}/countervailing-mode-with-friend")]
        [ProducesResponseType(typeof(RoomIn1vs1Response), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateRoomPlayCountervailingWithFriend(int accountId1, int gameId, int accountId2)
        {
            var rs = await _mediator.Send(new CreateRoomPlayCountervailingWithFriendCommand(gameId, accountId1, accountId2));
            return Ok(rs);
        }
        /// <summary>
        /// Remove Account From Queue
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="gameId"></param>
        /// <param name="coin"></param>
        /// <param name="roomOfAccount1vs1Id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{gameId:int},{coin:int},{roomOfAccount1vs1Id},{delay:int}/account-removed")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RemoveAccountFromQueue(int accountId,  int gameId,  int coin, string roomOfAccount1vs1Id, int delay)
        {
            var rs = await _mediator.Send(new RemoveAccountFromQueueCommand(accountId, coin, roomOfAccount1vs1Id, delay, gameId));
            return Ok(rs);
        }
        /// <summary>
        /// Start Room To Play Game
        /// </summary>
        /// <param name="room1vs1Id"></param>
        /// <param name="isUser1"></param>
        /// <param name="time"></param>
        /// <param name="progressTime"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{room1vs1Id},{isUser1},{time:int},{progressTime:int}/started-room")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToStartRoom(string room1vs1Id, bool isUser1, int time, int progressTime)
        {
            var rs = await _mediator.Send(new StartRoomIn1vs1Command(room1vs1Id, isUser1, time, progressTime));
            return Ok(rs);
        }
        /// <summary>
        /// Remove Room In 1vs1 In Real-time Database
        /// </summary>
        /// <param name="delayTime"></param>
        /// <param name="roomOfAccount1vs1Id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{delayTime:int},{roomOfAccount1vs1Id}/room-1vs1-removed")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RemoveRoom1vs1InRealtimeDatabase(string roomOfAccount1vs1Id, int delayTime)
        {
            var rs = await _mediator.Send(new RemoveRoom1vs1InRealtimeDatabaseCommand(roomOfAccount1vs1Id,delayTime));
            return Ok(rs);
        }
    }
}
