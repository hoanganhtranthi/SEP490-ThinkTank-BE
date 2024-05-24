using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        /// <summary>
        /// Get list of accounts
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountRequest"></param>
        /// <returns></returns>
       //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> GetAccounts([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountRequest accountRequest)
        {
            var rs = await _accountService.GetAccounts(accountRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
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
        /// Ban Account
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{accId:int}/account-banned")]
        public async Task<ActionResult<AccountResponse>> GetToBanAccount(int accId)
        {
            var rs = await _accountService.GetToBanAccount(accId);
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
        /// Login for role player (Username and Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authentication-player")]
        public async Task<ActionResult<AccountResponse>> LoginPlayer([FromBody] LoginRequest model)
        {
            var rs = await _accountService.LoginPlayer(model);
            return Ok(rs);
        }
        /// <summary>
        /// Get account to login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="googleId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("authentication-checking")]
        public async Task<ActionResult<AccountResponse>> GetAccountToLogin([FromQuery] string? username,[FromQuery] string? password, [FromQuery] string? googleId)
        {
            var rs = await _accountService.GetAccountToLogin(username,password,googleId);
            return Ok(rs);
        }
        /// <summary>
        /// Login for role admin (Username and Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authentication-admin")]
        public async Task<ActionResult<AccountResponse>> LoginAdmin([FromBody] LoginRequest model)
        {
            var rs = await _accountService.LoginAdmin(model);
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
        /// <param name="userId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{userId:int}/token-revoke")]
        public async Task<ActionResult<AccountResponse>> RevokeRefreshToken(int userId)
        {
            var rs = await _accountService.RevokeRefreshToken(userId);
            return Ok(rs);
        }

        /// <summary>
        /// Login Google 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("google-authentication")]
        public async Task<ActionResult<AccountResponse>> LoginGoogle([FromBody] LoginGoogleRequest request)
        {
            var rs = await _accountService.LoginGoogle(request);
            return Ok(rs);
        }
        /// <summary>
        /// Get current game level of account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{accountId:int}/game-level-of-account")]
        public async Task<ActionResult<List<GameLevelOfAccountResponse>>> GetGameLevelByAccountId( int accountId)
        {
            var rs = await _accountService.GetGameLevelByAccountId(accountId);
            return Ok(rs);
        }
    }
}
