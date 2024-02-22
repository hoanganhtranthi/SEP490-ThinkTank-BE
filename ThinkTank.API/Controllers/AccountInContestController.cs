using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accountincontests")]
    [ApiController]
    public class AccountInContestController : ControllerBase
    {
        private readonly
            IAccountInContestService _accountInContestService;

        public AccountInContestController(IAccountInContestService accountInContestService)
        {
            _accountInContestService = accountInContestService;
        }

        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AccountInContestResponse>>> GetAccountInContests([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountInContestRequest accountInContestRequest)
        {
            var rs = await _accountInContestService.GetAccountInContests(accountInContestRequest, pagingRequest);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpGet("result-contest-of-account")]
        public async Task<ActionResult<AccountInContestResponse>> GetContest([FromQuery] AccountInContestRequest accountInContestRequest)
        {
            var rs = await _accountInContestService.GetAccountInContest(accountInContestRequest);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<AccountInContestResponse>> CreateAccountInContest([FromBody] CreateAccountInContestRequest request)
        {
            var rs = await _accountInContestService.CreateAccountInContest(request);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AccountInContestResponse>> UpdateAccountInContest([FromBody] UpdateAccountInContestRequest request, int id)
        {
            var rs = await _accountInContestService.UpdateAccountInContest(id, request);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
