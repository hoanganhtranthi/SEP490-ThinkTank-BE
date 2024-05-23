
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IReportService
    {
        Task<PagedResults<ReportResponse>> GetReports(ReportRequest request, PagingRequest paging);
        Task<ReportResponse> GetReportById(int id);
        Task<ReportResponse> CreateReport(CreateReportRequest createReportRequest);
    }
}
