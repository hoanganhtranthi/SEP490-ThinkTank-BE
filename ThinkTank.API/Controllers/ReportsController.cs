using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Reports.Commands.CreateReport;
using ThinkTank.Application.CQRS.Reports.Queries.GetReports;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IMediator _mediator;
        public ReportsController(IMediator mediator)
        {
            _mediator= mediator;
        }
        /// <summary>
        /// Get list of reports
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<ReportResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetReports([FromQuery] PagingRequest pagingRequest, [FromQuery] ReportRequest reportRequest)
        {
            var rs = await _mediator.Send(new GetReportsQuery(pagingRequest,reportRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Create report
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpPost()]
        [ProducesResponseType(typeof(ReportResponse), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ReportResponse>> AddReport([FromBody] CreateReportRequest report)
        {
            var rs = await _mediator.Send(new CreateReportCommand(report));
            return Ok(rs);
        }
    }
}
