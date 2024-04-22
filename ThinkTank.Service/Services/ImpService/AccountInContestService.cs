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
        private readonly DateTime date;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        public AccountInContestService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }
        public async Task<AccountInContestResponse> CreateAccountInContest(CreateAndUpdateAccountInContestRequest request)
        {
            try
            {
                if (request.ContestId <= 0 || request.AccountId <= 0 || request.Duration < 0 || request.Mark < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var acc = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId);
                if (acc == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} Not Found!!!!!", "");
                }
                if (acc.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} Not Available!!!!!", "");
                }
                var contest=_unitOfWork.Repository<Contest>().Find(x=>x.Id==request.ContestId);
                if(contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Contest Id {request.ContestId} Not Found!!!!!", "");

                if (acc.Coin < contest.CoinBetting)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin for this contest", "");

                acc.Coin -= contest.CoinBetting;

                var accountInContest = _mapper.Map<AccountInContest>(request);
                accountInContest.AccountId = acc.Id;
                accountInContest.ContestId = request.ContestId;
                accountInContest.Contest = contest;
                await _unitOfWork.Repository<AccountInContest>().CreateAsync(accountInContest);

                //Update lại coin trong badge Tycoon của account nếu chưa đạt đủ 4000 coin
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == acc.Id && x.Challenge.Name.Equals("The Tycoon"));
                if (badge.CompletedDate == null  && badge.CompletedLevel < badge.Challenge.CompletedMilestone)
                {
                    badge.CompletedLevel = (int)acc.Coin;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }

                await _unitOfWork.Repository<Account>().Update(acc, acc.Id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<AccountInContestResponse>(accountInContest);
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
        private async Task<List<Badge>> GetListBadgesCompleted(Account account)
        {
            var result = new List<Badge>();
            var badges = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).Where(x => x.AccountId == account.Id).ToList();
            if (badges.Any())
            {
                foreach (var badge in badges)
                {
                    if (badge.CompletedLevel == badge.Challenge.CompletedMilestone)
                        result.Add(badge);
                }
            }
            return result;
        }
        public async Task<AccountInContestResponse> UpdateAccountInContest(CreateAndUpdateAccountInContestRequest request)
        {
            try
            {
                if (request.ContestId <= 0 || request.AccountId <= 0 || request.Duration < 0 || request.Mark < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var accountInContest = _unitOfWork.Repository<AccountInContest>()
                    .Find(s => s.ContestId == request.ContestId && s.AccountId == request.AccountId);
                if (accountInContest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Account in Contest is not found !!!", "");
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

                 _mapper.Map<CreateAndUpdateAccountInContestRequest, AccountInContest>(request,accountInContest);
                accountInContest.Prize = request.Mark / 10;
                accountInContest.CompletedTime = date;
                a.Coin += accountInContest.Prize;

                await _unitOfWork.Repository<AccountInContest>().Update(accountInContest,accountInContest.Id);
                await _unitOfWork.Repository<Account>().Update(a, request.AccountId);
                
                //Get badge
                await GetBadge(a, "The Tycoon");
                await GetBadge(a, "Super enthusiastic");

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInContestResponse>(accountInContest);
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
            //Get list badge mà account đó đã hoàn thành
            var result = await GetListBadgesCompleted(account);

            if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                if (badge != null)
                {
                    if (badge.CompletedLevel < challage.CompletedMilestone)
                    {
                        if (name.Equals("The Tycoon"))
                            badge.CompletedLevel = account.Coin < challage.CompletedMilestone ? (int)account.Coin : challage.CompletedMilestone;
                        else badge.CompletedLevel += 1;
                    }
                    if (badge.CompletedLevel == challage.CompletedMilestone)
                    {
                        badge.CompletedDate = date;

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
                            DateNotification = date,
                            Description = $"You have received {challage.Name} badge.",
                            Status = false,
                            Title = "ThinkTank"
                        };
                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                    }
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }
                else
                {
                    badge = new Badge();
                    badge.AccountId = account.Id;
                    badge.CompletedLevel = 1;
                    badge.ChallengeId = challage.Id;
                    badge.Status = false;
                    await _unitOfWork.Repository<Badge>().CreateAsync(badge);
                }
            }
        }
        public async Task<AccountInContestResponse> GetAccountInContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account In Contest Invalid", "");
                }
                var response = _unitOfWork.Repository<AccountInContest>().GetAll().AsNoTracking().Include(x => x.Account)
                    .Include(x => x.Contest).SingleOrDefault(x => x.Id == id);

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
                    var accountInContests = _unitOfWork.Repository<AccountInContest>().GetAll().AsNoTracking()
                    .Include(x => x.Account).Include(x => x.Contest)
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
