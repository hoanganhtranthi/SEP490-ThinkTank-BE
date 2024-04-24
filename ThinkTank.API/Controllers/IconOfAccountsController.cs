using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/iconOfAccounts")]
    [ApiController]
    public class IconOfAccountsController : Controller
    {
        private readonly IIconOfAccountService _iconOfAccountService;
        public IconOfAccountsController(IIconOfAccountService iconOfAccountService)
        {
            _iconOfAccountService = iconOfAccountService;
        }
        /// <summary>
        /// Get list of icon of account by account Id 
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="iconOfAccountRequest"></param>
        /// <returns></returns>
       [Authorize(Policy = "Player")]
        [HttpGet]
        public async Task<ActionResult<List<IconOfAccountResponse>>> GetIconOfAccounts([FromQuery] PagingRequest pagingRequest, [FromQuery] IconOfAccountRequest iconOfAccountRequest)
        {
            var rs = await _iconOfAccountService.GetIconOfAccounts(iconOfAccountRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get icon of account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IconOfAccountResponse>> GetIconOfAccountById(int id)
        {
            var rs = await _iconOfAccountService.GetIconOfAccountById(id);
            return Ok(rs);
        }

        /// <summary>
        /// Buy Icon
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost]
        public async Task<ActionResult<IconOfAccountResponse>> CreateIconOfAccount([FromBody] CreateIconOfAccountRequest request)
        {
            var rs = await _iconOfAccountService.CreateIconOfAccount(request);
            return Ok(rs);
        }
    }
}
