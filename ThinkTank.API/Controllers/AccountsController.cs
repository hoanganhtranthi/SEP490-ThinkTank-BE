using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService=accountService;
        }
        /// <summary>
        /// Get list of accounts
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="userRequest"></param>
        /// <returns></returns>
        
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> GetAccounts([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountRequest accountRequest)
        {
            var rs = await _accountService.GetAccounts(accountRequest,pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccountResponse>> GetAccount(int id)
        {
            var rs = await _accountService.GetAccountById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Update profile of account
        /// </summary>
        /// <param name="userRequest"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AccountResponse>> UpdateAccount([FromBody] UpdateAccountRequest userRequest, int id)
        {
            var rs = await _accountService.UpdateAccount(id, userRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
        /// <summary>
        /// Update Status Of Account (Online/Offline)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{accId:int}/status")]
        public async Task<ActionResult<AccountResponse>> GetToUpdateStatus(int accId)
        {
            var rs = await _accountService.GetToUpdateStatus(accId);
            return Ok(rs);
        }
        /// <summary>
        /// Ban Account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{accId:int}/account-banned")]
        public async Task<ActionResult<AccountResponse>> GetToBanAccount(int accId)
        {
            var rs = await _accountService.GetToBanAccount(accId);
            return Ok(rs);
        }
        /// <summary>
        /// Send Verification Code  
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("verification-code")]
        public async Task<ActionResult<string>> Verification([FromQuery] string email)
        {
            var rs = await _accountService.CreateMailMessage(email);
            return Ok(rs);
        }
        /// <summary>
        /// Sign Up account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost()]
        public async Task<ActionResult<AccountResponse>> CreateCustomer([FromBody] CreateAccountRequest account)
        {
            var rs = await _accountService.CreateAccount(account);
            return Ok(rs);
        }
        /// <summary>
        /// Login (Username and Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authentication")]
        public async Task<ActionResult<AccountResponse>> Login([FromBody] LoginRequest model)
        {
            var rs = await _accountService.Login(model);
            return Ok(rs);
        }
        /// <summary>
        /// Reset password when forgot password
        /// </summary>
        /// <param name="resetPassword"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("forgotten-password")]
        public async Task<ActionResult<AccountResponse>> ResetPassword([FromQuery] ResetPasswordRequest resetPassword)
        {
            var rs = await _accountService.UpdatePass(resetPassword);
            return Ok(rs);
        }

        /// <summary>
        /// Generate new access token and refresh token when they are expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("token-verification")]
        public async Task<ActionResult<AccountResponse>> VerifyAndGenerateToken(TokenRequest request)
        {
            var rs = await _accountService.VerifyAndGenerateToken(request);
            return Ok(rs);
        }

        /// <summary>
        /// Revoke refresh token when logout 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("token-revoke")]
        public async Task<ActionResult<AccountResponse>> RevokeRefreshToken(string userName)
        {
            var rs = await _accountService.RevokeRefreshToken(userName);
            return Ok(rs);
        }

        /// <summary>
        /// Login Google 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("google-authentication")]
        public async Task<ActionResult<AccountResponse>> LoginGoogle([FromQuery] string googleId)
        {
            var rs = await _accountService.LoginGoogle(googleId);
            return Ok(rs);
        }
    }
}
