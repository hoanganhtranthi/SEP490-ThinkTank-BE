

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Reports.Queries.GetReports
{
    public class GetReportsQuery:IGetTsQuery<PagedResults<ReportResponse>>
    {
        public ReportRequest ReportRequest { get; }
        public GetReportsQuery(PagingRequest pagingRequest,ReportRequest reportRequest) : base(pagingRequest)
        {
            ReportRequest = reportRequest;
        }
        
    }
}
