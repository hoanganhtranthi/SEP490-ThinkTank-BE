using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;
using System.Security.Principal;
using Firebase.Auth;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountInContestService : IAccountInContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        public AccountInContestService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }
        public async Task<AccountResponse> MinusCoinOfAccount(int id, int contestId)
        {
            try
            {
                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == id);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {id} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} Not Available!!!!!", "");
                }
                var contest=_unitOfWork.Repository<Contest>().Find(x=>x.Id==contestId);
                if(contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Contest Id {contestId} Not Found!!!!!", "");
                a.Coin -= contest.CoinBetting;
                if (a.Coin < contest.CoinBetting)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin for this contest", "");
                await _unitOfWork.Repository<Account>().Update(a, id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<AccountResponse>(a);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Minus coin of account error!!!", ex?.Message);
            }
        }
        public async Task<AccountInContestResponse> CreateAccountInContest(CreateAccountInContestRequest request)
        {
            try
            {
                var acc = _mapper.Map<CreateAccountInContestRequest, AccountInContest>(request);
                var s = _unitOfWork.Repository<AccountInContest>().Find(s => s.ContestId == request.ContestId && s.AccountId == request.AccountId);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Account in Contest has already !!!", "");
                }

                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Account Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Account Not Available!!!!!", "");
                }
                var c = _unitOfWork.Repository<Contest>().Find(c => c.Id == request.ContestId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Contest Not Found!!!!!", "");
                }
                if (c.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest Not Available!!!!!", "");
                }
                acc.Prize = request.Mark / 10;
                acc.CompletedTime = DateTime.Now;
                a.Coin += acc.Prize;
                await _unitOfWork.Repository<AccountInContest>().CreateAsync(acc);
                await _unitOfWork.Repository<Account>().Update(a, request.AccountId);
                await GetBadge(a, "The Tycoon");
                List<string> fcmTokens = new List<string>();
                fcmTokens.Add(a.Fcm);
                if (fcmTokens.Any())
                    _firebaseMessagingService.Subcribe(fcmTokens, $"/topics/contestId{c.Id}");
               await GetBadge(a, "Super enthusiastic");
               await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInContestResponse>(acc);
                rs.ContestName = c.Name;
                rs.UserName = a.UserName;
                rs.Avatar = a.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Account In Contest Error!!!", ex?.Message);
            }
        }
        private async Task GetBadge(Account account, string name)
        {
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Title == $"You have received {challage.Name} badge.");
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                {
                    if (name.Equals("The Tycoon"))
                        badge.CompletedLevel = (int)account.Coin;
                    else badge.CompletedLevel += 1;
                }
                if (badge.CompletedLevel == challage.CompletedMilestone && noti == null )
                {
                    badge.CompletedDate = DateTime.Now;
                   
                    #region send noti for account
                    List<string> fcmTokens = new List<string>();
                    if (account.Fcm != null)
                        fcmTokens.Add(account.Fcm);
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
                                                               new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank", Body = $"You have received {challage.Name} badge.", ImageUrl = challage.Avatar }, data);
                    #endregion
                    Notification notification = new Notification
                    {
                        AccountId = account.Id,
                        Avatar = challage.Avatar,
                        DateNotification = DateTime.Now,
                        Description = $"You have received {challage.Name} badge.",
                        Status=false, 
                        Title = "ThinkTank"
                    };
                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
                await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
            }
            else
            {
                CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                createBadgeRequest.AccountId = account.Id;
                createBadgeRequest.CompletedLevel =  1;
                createBadgeRequest.ChallengeId = challage.Id;
                var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                b.Status = false;
                await _unitOfWork.Repository<Badge>().CreateAsync(b);
            }
        }
        public async Task<AccountInContestResponse> GetAccountInContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Achievement Invalid", "");
                }
                var response = _unitOfWork.Repository<AccountInContest>().GetAll().Include(x => x.Account)
                    .Include(x => x.Contest).Include(x=>x.Contest.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account in contest with id {id}", "");
                }
                var rs = _mapper.Map<AccountInContestResponse>(response);
                rs.ContestName = response.Contest.Name;
                rs.UserName = response.Account.UserName;
                rs.Avatar = response.Account.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account In Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AccountInContestResponse>> GetAccountInContests(AccountInContestRequest request, PagingRequest paging)
        {
            try
            {
                    var filter = _mapper.Map<AccountInContestResponse>(request);
                    var accountInContests = _unitOfWork.Repository<AccountInContest>().GetAll().Include(x => x.Account).Include(x => x.Contest)
                    .Select(x => new AccountInContestResponse
                    {
                        Id = x.Id,
                        UserName = x.Account.UserName,
                        ContestName = x.Contest.Name,
                        CompletedTime = x.CompletedTime,
                        Duration = x.Duration,
                        AccountId = x.AccountId,
                        ContestId=x.ContestId,
                        Mark = x.Mark,
                        Avatar=x.Account.Avatar,
                        Prize = x.Prize
                    }).DynamicFilter(filter).ToList();
                    var sort = PageHelper<AccountInContestResponse>.Sorting(paging.SortType, accountInContests, paging.ColName);
                    var result = PageHelper<AccountInContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                    return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest's result list error!!!!!", ex.Message);
            }
        }
    }
}
