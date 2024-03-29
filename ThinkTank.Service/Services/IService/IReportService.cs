﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IReportService
    {
        Task<PagedResults<ReportResponse>> GetReports(ReportRequest request, PagingRequest paging);
        Task<ReportResponse> GetReportById(int id);
        Task<ReportResponse> CreateReport(CreateReportRequest createReportRequest);
    }
}
