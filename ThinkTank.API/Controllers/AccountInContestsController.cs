using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountInContests")]
    [ApiController]
    public class AccountInContestsController : ControllerBase
    {
        private readonly
            IAccountInContestService _accountInContestService;

        public AccountInContestsController(IAccountInContestService accountInContestService)
        {
            _accountInContestService = accountInContestService;
        }

        /// <summary>
        /// Get list of account in contest 
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountInContestRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet]
        public async Task<ActionResult<List<AccountInContestResponse>>> GetAccountInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountInContestRequest accountInContestRequest)
        {
            var rs = await _accountInContestService.GetAccountInContests(accountInContestRequest, pagingRequest);
            return Ok(rs);
        }

        /// <summary>
        /// Get account in contest by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountInContestResponse>> GetAccountInContestById([FromQuery] int id)
        {
            var rs = await _accountInContestService.GetAccountInContestById(id);
            return Ok(rs);
        }

        /// <summary>
        /// Create Account In Contest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost]
        public async Task<ActionResult<AccountInContestResponse>> CreateAccountInContest([FromBody] CreateAccountInContestRequest request)
        {
            var rs = await _accountInContestService.CreateAccountInContest(request);
            return Ok(rs);
        }
    }
}
