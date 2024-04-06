using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/analyses")]
    [ApiController]
    public class AnalysesController : Controller
    {
        private readonly IAnalysisService _analysisService;
        public AnalysesController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }
        /// <summary>
        /// Get analysis of account by account Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{accountId}")]
        public async Task<ActionResult<dynamic>> GetAnalysisOfAccount(int accountId)
        {
            var rs = await _analysisService.GetAnalysisOfAccountId(accountId);
            return Ok(rs);
        }
        /// <summary>
        /// Get analysis of account by account Id and game Id
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet()]
        public async Task<ActionResult<dynamic>> GetAnalysisOfAccount([FromQuery] AnalysisRequest request)
        {
            var rs = await _analysisService.GetAnalysisOfAccountIdAndGameId(request);
            return Ok(rs);
        }
        /// <summary>
        /// Get analysis of each type of account's memory by account Id 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{accountId:int}/by-memory-type")]
        public async Task<ActionResult<dynamic>> GetAnalysisOfEachTypeOfMemoryOfAccount(int accountId)
        {
            var rs = await _analysisService.GetAnalysisOfMemoryTypeByAccountId(accountId);
            return Ok(rs);
        }
    }
}
