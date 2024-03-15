using AutoMapper;
using FirebaseAdmin.Messaging;
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
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;
using Microsoft.EntityFrameworkCore;
using Notification = ThinkTank.Data.Entities.Notification;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Utilities;
using Repository.Extensions;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountIn1vs1Service : IAccountIn1vs1Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly ICacheService _cacheService;
        private readonly IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService;
        public AccountIn1vs1Service(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService,ICacheService cacheService, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            _cacheService = cacheService;
            this.firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<AccountIn1vs1Response> CreateAccount1vs1(CreateAccountIn1vs1Request createAccount1vs1Request)
        {
            try
            {
                var accIn1vs1 = _mapper.Map<CreateAccountIn1vs1Request, AccountIn1vs1>(createAccount1vs1Request);
                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == createAccount1vs1Request.AccountId1);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {createAccount1vs1Request.AccountId1} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createAccount1vs1Request.AccountId1} Not Available!!!!!", "");
                }
                var a2 = _unitOfWork.Repository<Account>().Find(a => a.Id == createAccount1vs1Request.AccountId2);
                if (a2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {createAccount1vs1Request.AccountId2} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createAccount1vs1Request.AccountId2}  Not Available!!!!!", "");
                }
                var c = _unitOfWork.Repository<Topic>().GetAll().Include(x=>x.Game).SingleOrDefault(c => c.Id == createAccount1vs1Request.TopicId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Topic Not Found!!!!!", "");
                }
                if(createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId1 || createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Winner Id {createAccount1vs1Request.WinnerId} is invalid !!!!!", "");
                if (createAccount1vs1Request.WinnerId == a.Id)
                    a.Coin += (createAccount1vs1Request.Coin * 2);
                else a2.Coin += createAccount1vs1Request.Coin * 2;
                accIn1vs1.EndTime = DateTime.Now;
                await _unitOfWork.Repository<AccountIn1vs1>().CreateAsync(accIn1vs1);
                await _unitOfWork.Repository<Account>().Update(a, a.Id);
                await _unitOfWork.Repository<Account>().Update(a2, a2.Id);
                if (a.Coin == 4000)
                    GetBadge(a, "The Tycoon");
                if(a2.Coin ==4000)
                    GetBadge(a2, "The Tycoon");
                if (a.AccountIn1vs1AccountId1Navigations.Count() > 5 || a.AccountIn1vs1AccountId2Navigations.Count() > 5 || 
                    (a.AccountIn1vs1AccountId2Navigations.Count()+a.AccountIn1vs1AccountId1Navigations.Count())>5)
                {
                    GetBadge(a, "Athlete");
                }
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountIn1vs1Response>(accIn1vs1);
                rs.GameName = c.Game.Name;
                rs.Username1 = a.UserName;
                rs.Username2 = a2.UserName;

                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Account In 1vs1 Error!!!", ex?.Message);
            }
        }

        private async Task GetBadge(Account account, string name)
        {
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                    badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone)
                {
                    badge.CompletedDate = DateTime.Now;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
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
                        DateTime = DateTime.Now,
                        Description = $"You have received {challage.Name} badge.",
                        Status = false,
                        Title = "ThinkTank"
                    };
                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
            }
            else
            {
                CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                createBadgeRequest.AccountId = account.Id;
                createBadgeRequest.CompletedLevel = 1;
                createBadgeRequest.ChallengeId = challage.Id;
                var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                b.Status = false;
                await _unitOfWork.Repository<Badge>().CreateAsync(b);
            }
        }
        public async Task<AccountIn1vs1Response> GetAccount1vs1ById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account 1vs1 Invalid", "");
                }
                var response = _unitOfWork.Repository<AccountIn1vs1>().GetAll().Include(x => x.AccountId1Navigation)
                    .Include(x => x.AccountId2Navigation).Include(x=>x.Topic).SingleOrDefault(x=>x.Id==id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account 1vs1 with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AccountIn1vs1Response>(response);
                rs.Username1 = response.AccountId1Navigation.UserName;
                rs.Username2 = response.AccountId2Navigation.UserName;
                rs.GameName = response.Topic.Game.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account 1vs1 By ID Error!!!", ex.InnerException?.Message);
            }
        }
      /* public async Task<int> FindAccountTo1vs1(int id, int coin)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account 1vs1 Invalid", "");
                }
                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == id);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {id} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} Not Available!!!!!", "");
                }
                var list = await _cacheService.GetJobsAsync("account1vs1");
                if (list != null)
                {
                    foreach (var account in list)
                    {
                        var c = account.Split('+')[1];
                        if (Int16.Parse(c) == coin)
                        {
                            return Int16.Parse(account.Split('+')[0]);
                        }
                    }
                }
                else
                {
                    _cacheService.AddJobAsync<string>($"{id}+{coin}", "account1vs1");
                    return 0;
                }
                
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account 1vs1 By ID Error!!!", ex.InnerException?.Message);
            }
        }*/

        public async Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AccountIn1vs1Response>(request);
                var account1vs1s = _unitOfWork.Repository<AccountIn1vs1>().GetAll().Include(x => x.AccountId1Navigation)
                 .Include(x=>x.AccountId2Navigation).Include(x => x.Topic.Game).Include(x => x.Topic)
                    .Select(x => new AccountIn1vs1Response
                    {
                        Id = x.Id,
                        GameName = x.Topic.Game.Name,
                        AccountId2=x.AccountId2,
                        AccountId1=x.AccountId1,
                        Coin=x.Coin,
                        EndTime=x.EndTime,
                        StartTime=x.StartTime,
                        GameId=(int)x.Topic.GameId,
                        TopicId=(int)x.TopicId,
                        Username1=x.AccountId1Navigation.UserName,
                        Username2=x.AccountId2Navigation.UserName,
                        WinnerId=x.WinnerId
                    }).DynamicFilter(filter).ToList();
                if (request.GameId != null)
                    account1vs1s = account1vs1s.Where(x => x.GameId == request.GameId).ToList();
                var sort = PageHelper<AccountIn1vs1Response>.Sorting(paging.SortType, account1vs1s, paging.ColName);
                var result = PageHelper<AccountIn1vs1Response>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get accounts 1vs1 list error!!!!!", ex.Message);
            }
        }
    }
}
