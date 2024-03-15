using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        /// <summary>
        /// Get list of reports
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<ReportResponse>>> GetReports([FromQuery] PagingRequest pagingRequest, [FromQuery] ReportRequest reportRequest)
        {
            var rs = await _reportService.GetReports(reportRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get report by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReportResponse>> GetReport(int id)
        {
            var rs = await _reportService.GetReportById(id);
            return Ok(rs);
        }
        /// <summary>
        /// Create report
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
      [Authorize(Policy = "Player")]
        [HttpPost()]
        public async Task<ActionResult<ReportResponse>> AddReport([FromBody] CreateReportRequest report)
        {
            var rs = await _reportService.CreateReport(report);
            return Ok(rs);
        }
    }
}
