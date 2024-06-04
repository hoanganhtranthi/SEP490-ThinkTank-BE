

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Reports.Commands.CreateReport
{
    public class CreateReportCommand:ICommand<ReportResponse>
    {
        public CreateReportRequest CreateReportRequest { get; }
        public CreateReportCommand(CreateReportRequest createReportRequest)
        {
            CreateReportRequest = createReportRequest;
        }
    }
}
