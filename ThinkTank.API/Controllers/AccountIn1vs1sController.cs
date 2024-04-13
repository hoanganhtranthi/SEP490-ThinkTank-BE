using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountIn1vs1")]
    [ApiController]
    public class AccountIn1vs1sController : Controller
    {
       private readonly IAccountIn1vs1Service accountIn1Vs1Service;
        public AccountIn1vs1sController(IAccountIn1vs1Service accountIn1Vs1Service)
        {
            this.accountIn1Vs1Service = accountIn1Vs1Service;
        }
        /// <summary>
        /// Get list of account in 1vs1 
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountIn1Vs1Request"></param>
        /// <returns></returns>
         [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AccountIn1vs1Response>>> GetAccountIn1vs1s([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountIn1vs1Request accountIn1Vs1Request)
        {
            var rs = await accountIn1Vs1Service.GetAccount1vs1s(accountIn1Vs1Request, pagingRequest);
            return Ok(rs);
        }

        /// <summary>
        /// Get account in 1vs1 by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountInContestResponse>> GetAccountIn1vs1ById( int id)
        {
            var rs = await accountIn1Vs1Service.GetAccount1vs1ById(id);
            return Ok(rs);
        }

        /// <summary>
        /// Create Account In 1vs1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
     [Authorize(Policy = "Player")]
        [HttpPost]
        public async Task<ActionResult<AccountInContestResponse>> CreateAccountIn1vs1([FromBody] CreateAccountIn1vs1Request request)
        {
            var rs = await accountIn1Vs1Service.CreateAccount1vs1(request);
            return Ok(rs);
        }
        /// <summary>
        /// Find Opponent of Account In 1vs1
        /// </summary>
        /// <param name="accountId"></param>
        ///  <param name="gameId"></param>
        ///   <param name="coin"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{gameId:int},{coin:int}/opponent-of-account")]
        public async Task<ActionResult<dynamic>> FindAccountIn1vs1( int accountId,  int gameId,  int coin)
        {
            var rs = await accountIn1Vs1Service.FindAccountTo1vs1(accountId,coin,gameId);
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
        public async Task<ActionResult<dynamic>> CreateRoomPlayCountervailingWithFriend(int accountId1, int gameId, int accountId2)
        {
            var rs = await accountIn1Vs1Service.CreateRoomPlayCountervailingWithFriend(gameId, accountId1, accountId2);
            return Ok(rs);
        }
        /// <summary>
        /// Remove Account From Cache
        /// </summary>
        /// <param name="accountId"></param>
        ///  <param name="gameId"></param>
        ///   <param name="coin"></param>
        ///    <param name="roomOfAccount1vs1Id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int},{gameId:int},{coin:int},{roomOfAccount1vs1Id}/account-removed")]
        public async Task<ActionResult<bool>> RemoveAccountFromCache(int accountId,  int gameId,  int coin, string roomOfAccount1vs1Id)
        {
            var rs = await accountIn1Vs1Service.RemoveAccountFromCache(accountId, coin, gameId,roomOfAccount1vs1Id);
            return Ok(rs);
        }
    }
}
