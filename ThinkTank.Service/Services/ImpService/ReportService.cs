using AutoMapper;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Services.IService;
using Microsoft.EntityFrameworkCore;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        public ReportService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<ReportResponse> CreateReport(CreateReportRequest createReportRequest)
        {
            try
            {
                if (createReportRequest.AccountId1 == createReportRequest.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, "Add report Invalid !!!", "");

                var report = _mapper.Map<CreateReportRequest, Report>(createReportRequest);
                var s = _unitOfWork.Repository<Account>().Find(s => s.Id == createReportRequest.AccountId1);
                if (s == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createReportRequest.AccountId1} is not found !!!", "");
                }
                var cus = _unitOfWork.Repository<Account>().Find(s => s.Id == createReportRequest.AccountId2);
                if (cus == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createReportRequest.AccountId2} is not found !!!", "");
                }
                report.DateTime = DateTime.Now;
                await _unitOfWork.Repository<Report>().CreateAsync(report);
                if (s.Avatar == null)
                    s.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Flogo_2_bg%201%20%281%29.png?alt=media&token=437436e4-28ce-4a0c-a7d2-a8763064151f";
                #region send noti for account
                List<string> fcmTokens = new List<string>();
                fcmTokens.Add(cus.Fcm);
                var data = new Dictionary<string, string>()
                {
                    ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                    ["Action"] = "home",
                    ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    }),
                };
                if (fcmTokens.Any())
                    _firebaseMessagingService.SendToDevices(fcmTokens,
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Community", Body = $"{s.FullName} sent you a report request.", ImageUrl = s.Avatar }, data);
                #endregion            
                Notification notification = new Notification
                {
                    AccountId = cus.Id,
                    Avatar = s.Avatar,
                    DateTime = DateTime.Now,
                    Description = $"You have a report for acting {createReportRequest.Titile}.",
                    Titile = "ThinkTank Report"
                };
                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<ReportResponse>(report);
                rs.UserName1 = s.UserName;
                rs.UserName2 = cus.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Report Error!!!", ex?.Message);
            }
        }

        public async Task<ReportResponse> GetReportById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Report Invalid", "");
                }
                var response = _unitOfWork.Repository<Report>().GetAll().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found report with id {id.ToString()}", "");
                }

                var rs = _mapper.Map<ReportResponse>(response);
                rs.UserName1 = response.AccountId1Navigation.UserName;
                rs.UserName2 = response.AccountId2Navigation.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Report By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<ReportResponse>> GetReports(ReportRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<ReportResponse>(request);
                var friends = _unitOfWork.Repository<Report>().GetAll().Include(a => a.AccountId1Navigation)
                    .Include(a => a.AccountId2Navigation).Select(x => new ReportResponse
                    {
                        Id = x.Id,
                        AccountId1 = x.AccountId1,
                        AccountId2 = x.AccountId2,
                        Titile = x.Titile,
                        Description=x.Description,
                        UserName1 = x.AccountId1Navigation.UserName,
                        UserName2 = x.AccountId2Navigation.UserName,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<ReportResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<ReportResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get report list error!!!!!", ex.Message);
            }
        }
    }
}
