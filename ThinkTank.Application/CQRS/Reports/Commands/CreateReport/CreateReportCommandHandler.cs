

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Reports.Commands.CreateReport
{
    public class CreateReportCommandHandler : ICommandHandler<CreateReportCommand, ReportResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly INotificationService _notificationService;
        public CreateReportCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,  INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _notificationService = notificationService;
        }
        public async Task<ReportResponse> Handle(CreateReportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateReportRequest.AccountId1 == request.CreateReportRequest.AccountId2 ||request.CreateReportRequest.AccountId1 <= 0 || request.CreateReportRequest.AccountId2 <= 0
                || request.CreateReportRequest.Description == null || request.CreateReportRequest.Description == "" || request.CreateReportRequest.Title == null || request.CreateReportRequest.Title == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Add report Invalid !!!", "");
                var report = _mapper.Map<CreateReportRequest, Report>(request.CreateReportRequest);

                var acc1 = _unitOfWork.Repository<Account>().Find(s => s.Id == request.CreateReportRequest.AccountId1);
                if (acc1 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {request.CreateReportRequest.AccountId1} is not found !!!", "");
                }
                if (acc1.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc1.Id} is block", "");

                var acc2 = _unitOfWork.Repository<Account>().Find(s => s.Id == request.CreateReportRequest.AccountId2);
                if (acc2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {request.CreateReportRequest.AccountId2} is not found !!!", "");
                }

                if (acc2.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc2.Id} is block", "");

                var reportsWithinTimeframe = _unitOfWork.Repository<Report>()
                .GetAll()
                .AsNoTracking()
                .Where(x => x.AccountId1 == request.CreateReportRequest.AccountId1 && EF.Functions.DateDiffMinute(x.DateReport.Value, date) <= 10)
                .ToList();

                if (reportsWithinTimeframe.Count() >= 3)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "During 10 minutes, you can only send a maximum of 3 reports", "");
                }

                report.DateReport = date;
                await _unitOfWork.Repository<Report>().CreateAsync(report);

                if (acc1.Avatar == null)
                    acc1.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Flogo_2_bg%201%20%281%29.png?alt=media&token=437436e4-28ce-4a0c-a7d2-a8763064151f";

                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if (acc2.Fcm != null)
                    fcmTokens.Add(acc2.Fcm);
                await _notificationService.SendNotification(fcmTokens, $"You have a report for acting {request.CreateReportRequest.Title}.",
                    "ThinkTank Report", acc1.Avatar, acc2.Id);
                #endregion

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<ReportResponse>(report);
                rs.UserName1 = acc1.UserName;
                rs.UserName2 = acc2.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Report Error!!!", ex?.Message);
            }
        }
    }
}
