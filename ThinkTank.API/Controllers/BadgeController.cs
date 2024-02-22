using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/badges")]
    [ApiController]
    public class BadgeController : ControllerBase
    {
        private readonly
            IBadgeService _badgeService;

        public BadgeController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BadgeResponse>>> GetBadges([FromQuery] PagingRequest pagingRequest, [FromQuery] BadgeRequest badgeRequest)
        {
            var rs = await _badgeService.GetBadges(badgeRequest, pagingRequest);
            return Ok(rs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<List<BadgeResponse>>> GetBadgesIsCompleted([FromQuery] PagingRequest pagingRequest, [FromQuery] BadgeRequest badgeRequest)
        {
            var rs = await _badgeService.GetBadgesIsCompleted(badgeRequest, pagingRequest);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BadgeResponse>> CreateBadge([FromBody] CreateBadgeRequest badgeRequest)
        {
            var rs = await _badgeService.CreateBadge(badgeRequest);
            return Ok(rs);
        }

        //[Authorize(Policy = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<BadgeResponse>> UpdateBadge([FromBody] CreateBadgeRequest badgeRequest, int id)
        {
            var rs = await _badgeService.UpdateBadge(id, badgeRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
