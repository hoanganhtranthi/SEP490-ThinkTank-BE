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
using System.Security.Principal;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountIn1vs1Service : IAccountIn1vs1Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly ICacheService _cacheService;
        private readonly IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService;
        public AccountIn1vs1Service(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService, ICacheService cacheService, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
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
                var c = _unitOfWork.Repository<Game>().GetAll().SingleOrDefault(c => c.Id == createAccount1vs1Request.GameId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Game Not Found!!!!!", "");
                }
                if (createAccount1vs1Request.WinnerId != 0 && (createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId1 || createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId2))
                    throw new CrudException(HttpStatusCode.BadRequest, $"Winner Id {createAccount1vs1Request.WinnerId} is invalid !!!!!", "");
                var room=firebaseRealtimeDatabaseService.GetAsync <RoomsOfAccount1vs1>($"roomsOfAccount1vs1/roomId{createAccount1vs1Request.RoomId}").Result ;
                if(room != null)
                {
                    if (room.TimeId1 == 0 && room.TimeId2 == 0 || room.TimeId1 == room.TimeId2)
                    {
                        accIn1vs1.WinnerId = 0;
                        a.Coin = createAccount1vs1Request.Coin;
                        a2.Coin = createAccount1vs1Request.Coin;
                    }
                    if(room.TimeId1 ==0)
                    {
                            accIn1vs1.WinnerId = a.Id;
                            a.Coin += createAccount1vs1Request.Coin * 2;
                            #region send noti for account
                            List<string> fcmTokens = new List<string>();
                            if (a.Fcm != null)
                                fcmTokens.Add(a.Fcm);
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
                                                                       new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank 1vs1 Mode", Body = "Your opponent lost connection so you won" }, data);
                        #endregion
                    }
                    if (room.TimeId2 == 0)
                    {
                          accIn1vs1.WinnerId = a2.Id;
                         a2.Coin += (createAccount1vs1Request.Coin * 2);
                        #region send noti for account
                        List<string> fcmTokens = new List<string>();
                        if (a2.Fcm != null)
                            fcmTokens.Add(a2.Fcm);
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
                                                                   new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank 1vs1 Mode", Body = "Your opponent lost connection so you won" }, data);
                        #endregion
                    }
                    if (room.TimeId1 != 0 && room.TimeId2 != 0)
                    {
                        if (room.TimeId1 < room.TimeId2)
                        {
                            accIn1vs1.WinnerId = a.Id;
                            a.Coin += (createAccount1vs1Request.Coin * 2);
                        }
                        if (room.TimeId1 > room.TimeId2)
                        {
                            accIn1vs1.WinnerId = a2.Id;
                            a2.Coin += createAccount1vs1Request.Coin * 2;
                        }
                    }
                }
                accIn1vs1.EndTime = DateTime.Now;
                a.Coin -= createAccount1vs1Request.Coin;
                a2.Coin -= createAccount1vs1Request.Coin;
                await _unitOfWork.Repository<AccountIn1vs1>().CreateAsync(accIn1vs1);
                await _unitOfWork.Repository<Account>().Update(a, a.Id);
                await _unitOfWork.Repository<Account>().Update(a2, a2.Id);
                if (a.Coin == 4000)
                    GetBadge(a, "The Tycoon");
                if (a2.Coin == 4000)
                    GetBadge(a2, "The Tycoon");
                if (a.AccountIn1vs1AccountId1Navigations.Count() > 5 || a.AccountIn1vs1AccountId2Navigations.Count() > 5 ||
                    (a.AccountIn1vs1AccountId2Navigations.Count() + a.AccountIn1vs1AccountId1Navigations.Count()) > 5)
                {
                    GetBadge(a, "Athlete");
                }
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountIn1vs1Response>(accIn1vs1);
                rs.GameName = c.Name;
                rs.Username1 = a.UserName;
                rs.Username2 = a2.UserName;
                firebaseRealtimeDatabaseService.RemoveData<RoomsOfAccount1vs1>($"roomsOfAccount1vs1/roomId{createAccount1vs1Request.RoomId}");
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
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Title == $"You have received {challage.Name} badge.");
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                    badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone && noti == null)
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
                    .Include(x => x.AccountId2Navigation).Include(x => x.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account 1vs1 with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AccountIn1vs1Response>(response);
                rs.Username1 = response.AccountId1Navigation.UserName;
                rs.Username2 = response.AccountId2Navigation.UserName;
                rs.GameName = response.Game.Name;
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
        private async Task AddToCacheAsync(int id, int coin, int gameId)
        {
            var cacheKey = $"{id}+{coin}+{gameId}";
            var cacheResult = await _cacheService.GetJobsAsync(cacheKey);               
                if (cacheResult == null || !cacheResult.Any())
                {
                    await _cacheService.AddJobAsync(cacheKey, $"{coin}+{gameId}");
                }
        }

        private async Task RemoveFromCacheAsync(int coin, int gameId,string accountInfo)
        {
            await _cacheService.DeleteJobAsync($"{coin}+{gameId}",accountInfo);
        }

        public async Task<dynamic> FindAccountTo1vs1(int id, int coin, int gameId)
        {
            try
            {
                using (var redLock = _cacheService.AcquireLock($"{coin}+{gameId}"))
                {
                    if (redLock.IsAcquired)
                    {
                        await _semaphore.WaitAsync(); // Di chuyển semaphore vào bên trong để giảm thiểu thời gian chờ đợi
                        try
                        {
                            if (id <= 0)
                            {
                                throw new CrudException(HttpStatusCode.BadRequest, "Id Account 1vs1 Invalid", "");
                            }

                            var account = await _unitOfWork.Repository<Account>().FindAsync(a => a.Id == id);
                            if (account == null)
                            {
                                throw new CrudException(HttpStatusCode.NotFound, $"Account Id {id} Not Found!!!!!", "");
                            }

                            if (account.Status == false)
                            {
                                throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} Not Available!!!!!", "");
                            }

                            var game = await _unitOfWork.Repository<Game>().FindAsync(x => x.Id == gameId);
                            var accountId = 0;
                            var uniqueId = "";
                            var list = await _cacheService.GetJobsAsync($"{coin}+{gameId}");
                            if (list?.Any() == true)
                            {
                                foreach (var accountInfo in list)
                                {
                                    var accountValues = accountInfo.Split('+');

                                    var accountIdFromCache = Int32.Parse(accountValues[0]);
                                    var coinFromCache = Int32.Parse(accountValues[1]);
                                    var gameIdFromCache = Int32.Parse(accountValues[2]);

                                    if (coinFromCache == coin && gameIdFromCache == gameId && accountIdFromCache != id)
                                    {
                                        accountId = accountIdFromCache;
                                        uniqueId = Guid.NewGuid().ToString();
                                        await Create1vs1Room(id, accountId, uniqueId);
                                        await SendNotificationToAccounts(account, accountId, game, uniqueId);
                                        await RemoveFromCacheAsync(coin, gameId, accountInfo); // Xóa dữ liệu khỏi cache sau khi tìm thấy tài khoản

                                        break;
                                    }
                                }

                                if (accountId == 0)
                                {
                                    await AddToCacheAsync(id, coin, gameId).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await AddToCacheAsync(id, coin, gameId).ConfigureAwait(false);
                            }
                            return new
                            {
                                AccountId = accountId,
                                RoomId=uniqueId
                            };
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                }
                return new
                {
                    AccountId = 0,
                    RoomId = ""
                };
            }
            catch (CrudException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Find account 1vs1 Error!!!", ex.InnerException?.Message);
            }
        }
            private async Task Create1vs1Room(int accountId1, int accountId2,string uniqueId)
        {
            var roomAccount1vs1 = new RoomsOfAccount1vs1()
            {
                Id = uniqueId,
                AccountId1 = accountId1,
                AccountId2 = accountId2,
                TimeId1 = 0,
                TimeId2 = 0,
                Message = ""
            };
            await firebaseRealtimeDatabaseService.SetAsync<RoomsOfAccount1vs1>($"roomsOfAccount1vs1/roomId{uniqueId}", roomAccount1vs1);
        }

        private async Task SendNotificationToAccounts(Account account1, int accountId2, Game game,string uniqueId)
        {
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

            var fcmTokens = new Dictionary<int, string>();
            if (!string.IsNullOrEmpty(account1.Fcm))
            {
                fcmTokens.Add(account1.Id, account1.Fcm);
            }

            var account2 = await _unitOfWork.Repository<Account>().FindAsync(x => x.Id == accountId2);
            if (!string.IsNullOrEmpty(account2.Fcm))
            {
                fcmTokens.Add(account2.Id, account2.Fcm);
            }

            foreach (var token in fcmTokens)
            {
                var notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = "ThinkTank Account1vs1 Mode",
                    Body = $"Having found your opponent, you can start playing the game {game.Name} in countervailing mode",
               };

                var count = await _firebaseMessagingService.SendMessage(new List<string> { token.Value }, notification, data);

                if (count == 0 && fcmTokens.Count > 1)
                {
                    await SendNotificationForAccountNotFound(game, token.Key,uniqueId);
                }
            }

            if (fcmTokens.Count == 1)
            {
                // Gửi thông báo cho tài khoản còn lại (nếu có)
                var remainingToken = fcmTokens.First();
                var notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = "ThinkTank Account1vs1 Mode",
                    Body = $"Could not find an opponent for the game {game.Name}, please find another opponent.",
                };

                await _firebaseMessagingService.SendMessage(new List<string> { remainingToken.Value }, notification, data);
                await firebaseRealtimeDatabaseService.RemoveData<RoomsOfAccount1vs1>("roomsOfAccount1vs1/roomId" + uniqueId);
            }
        }

        private async Task SendNotificationForAccountNotFound(Game game, int accountId,string uniqueId)
        {
            var accountRequest = await _unitOfWork.Repository<Account>().FindAsync(x => x.Id == accountId);
            if (!string.IsNullOrEmpty(accountRequest.Fcm))
            {
                var dataFCM = new Dictionary<string, string>()
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
                var notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = "ThinkTank Account1vs1 Mode",
                    Body = $"Could not find an opponent for the game {game.Name}, please find another opponent.",
                };
                _firebaseMessagingService.SendToDevices(
                    new List<string> { accountRequest.Fcm }, notification, dataFCM);
              await  firebaseRealtimeDatabaseService.RemoveData<RoomsOfAccount1vs1>("roomsOfAccount1vs1/roomId" + uniqueId);
            }
        }



        public async Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AccountIn1vs1Response>(request);
                var account1vs1s = _unitOfWork.Repository<AccountIn1vs1>().GetAll().Include(x => x.AccountId1Navigation)
                 .Include(x => x.AccountId2Navigation).Include(x => x.Game)
                    .Select(x => new AccountIn1vs1Response
                    {
                        Id = x.Id,
                        GameName = x.Game.Name,
                        AccountId2 = x.AccountId2,
                        AccountId1 = x.AccountId1,
                        Coin = x.Coin,
                        EndTime = x.EndTime,
                        StartTime = x.StartTime,
                        GameId = (int)x.GameId,
                        Username1 = x.AccountId1Navigation.UserName,
                        Username2 = x.AccountId2Navigation.UserName,
                        WinnerId = x.WinnerId
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
