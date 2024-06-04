using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.Accounts.Commands.BanAccount;
using ThinkTank.Application.Accounts.Commands.CreateAccount;
using ThinkTank.Application.Accounts.Commands.ForgotPassword;
using ThinkTank.Application.Accounts.Commands.Login;
using ThinkTank.Application.Accounts.Commands.LoginGoogle;
using ThinkTank.Application.Accounts.Commands.Logout;
using ThinkTank.Application.Accounts.Commands.UpdateAccount;
using ThinkTank.Application.Accounts.Commands.VerifyAndGenerateToken;
using ThinkTank.Application.Accounts.Queries.GetAccountById;
using ThinkTank.Application.Accounts.Queries.GetAccounts;
using ThinkTank.Application.Accounts.Queries.GetAccountToLogin;
using ThinkTank.Application.Accounts.Queries.GetGameLevelByAccountId;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : Controller
    {
        private readonly IMediator _mediator;
        public AccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get list of accounts
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="accountRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<AccountResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccounts([FromQuery] PagingRequest pagingRequest, [FromQuery] AccountRequest accountRequest)
        {
            var rs = await _mediator.Send(new GetAccountsQuery(pagingRequest,accountRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Get account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccount(int id)
        {
            var rs = await _mediator.Send(new GetAccountByIdQuery(id));
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
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest userRequest, int id)
        {
            var rs = await _mediator.Send(new UpdateAccountCommand(id, userRequest));
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
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToBanAccount(int accId)
        {
            var rs = await _mediator.Send(new BanAccountCommand(accId));
            return Ok(rs);
        }
        /// <summary>
        /// Sign Up account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateAccountRequest account)
        {
            var rs = await _mediator.Send(new CreateAccountCommand(account));
            return Ok(rs);
        }
        /// <summary>
        /// Login for role player (Username and Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authentication-player")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginPlayer([FromBody] LoginRequest model)
        {
            var rs = await _mediator.Send(new LoginPlayerCommand(model));
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
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccountToLogin([FromQuery] string? username,[FromQuery] string? password, [FromQuery] string? googleId)
        {
            var rs = await _mediator.Send(new GetAccountToLoginQuery(username,password,googleId));
            return Ok(rs);
        }
        /// <summary>
        /// Login for role admin (Username and Password)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authentication-admin")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest model)
        {
            var rs = await _mediator.Send(new LoginAdminCommand(model));
            return Ok(rs);
        }
        /// <summary>
        /// Reset password when forgot password
        /// </summary>
        /// <param name="resetPassword"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("forgotten-password")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordRequest resetPassword)
        {
            var rs = await _mediator.Send(new ForgotPasswordCommand(resetPassword));
            return Ok(rs);
        }

        /// <summary>
        /// Generate new access token and refresh token when they are expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("token-verification")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> VerifyAndGenerateToken(TokenRequest request)
        {
            var rs = await _mediator.Send(new VerifyAndGenerateTokenCommand(request));
            return Ok(rs);
        }

        /// <summary>
        /// Revoke refresh token when logout 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{userId:int}/token-revoke")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RevokeRefreshToken(int userId)
        {
            var rs = await _mediator.Send(new RevokeRefreshTokenCommand(userId));
            return Ok(rs);
        }

        /// <summary>
        /// Login Google 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("google-authentication")]
        [ProducesResponseType(typeof(AccountResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginGoogle([FromBody] LoginGoogleRequest request)
        {
            var rs = await _mediator.Send(new LoginGoogleCommand(request));
            return Ok(rs);
        }
        /// <summary>
        /// Get current game level of account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "All")]
        [HttpGet("{accountId:int}/game-level-of-account")]
        [ProducesResponseType(typeof(List<GameLevelOfAccountResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGameLevelByAccountId( int accountId)
        {
            var rs = await _mediator.Send(new GetGameLevelByAccountIdQuery(accountId));
            return Ok(rs);
        }
    }
}
