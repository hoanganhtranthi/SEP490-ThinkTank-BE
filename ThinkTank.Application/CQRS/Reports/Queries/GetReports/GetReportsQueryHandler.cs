
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.Reports.Queries.GetReports
{
    public class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, PagedResults<ReportResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;

        public GetReportsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }

        public async Task<PagedResults<ReportResponse>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var filter = _mapper.Map<ReportResponse>(request.ReportRequest);
                var reports = _unitOfWork.Repository<Report>().GetAll().AsNoTracking().Include(a => a.AccountId1Navigation)
                    .Include(a => a.AccountId2Navigation).Select(x => new ReportResponse
                    {
                        Id = x.Id,
                        AccountId1 = x.AccountId1,
                        AccountId2 = x.AccountId2,
                        DateReport = x.DateReport,
                        Title = x.Title,
                        Description = x.Description,
                        UserName1 = x.AccountId1Navigation.UserName,
                        UserName2 = x.AccountId2Navigation.UserName,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<ReportResponse>.Sorting(request.PagingRequest.SortType, reports, request.PagingRequest.ColName);
                var result = PageHelper<ReportResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get report list error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get report list error!!!!!", ex.Message);
            }
        }
    }
}
