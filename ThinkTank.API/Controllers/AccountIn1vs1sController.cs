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
         [Authorize(Policy = "All")]
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
        [HttpGet("opponent-of-account")]
        public async Task<ActionResult<int>> FindAccountIn1vs1([FromQuery] int accountId, [FromQuery] int gameId, [FromQuery] int coin)
        {
            var rs = await accountIn1Vs1Service.FindAccountTo1vs1(accountId,coin,gameId);
            return Ok(rs);
        }
    }
}
