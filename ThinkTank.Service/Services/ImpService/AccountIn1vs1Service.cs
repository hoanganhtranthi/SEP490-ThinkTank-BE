﻿using AutoMapper;
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
using Firebase.Auth;

namespace ThinkTank.Service.Services.ImpService
{
    public class AccountIn1vs1Service : IAccountIn1vs1Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
        private readonly DateTime date;
        public AccountIn1vs1Service(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<AccountIn1vs1Response> CreateAccount1vs1(CreateAccountIn1vs1Request createAccount1vs1Request)
        {
            try
            {
                if (createAccount1vs1Request.AccountId1 <= 0 || createAccount1vs1Request.AccountId2 <= 0 || createAccount1vs1Request.Coin <= 0
                    || createAccount1vs1Request.RoomOfAccountIn1vs1Id == null || createAccount1vs1Request.RoomOfAccountIn1vs1Id == "" || createAccount1vs1Request.WinnerId <= 0
                    || createAccount1vs1Request.StartTime == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid","");

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
  
                if (createAccount1vs1Request.WinnerId != 0 && createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId1  && createAccount1vs1Request.WinnerId != createAccount1vs1Request.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Winner Id {createAccount1vs1Request.WinnerId} is invalid !!!!!", "");
                var accountIn1vs1 = _unitOfWork.Repository<AccountIn1vs1>().Find(x => x.AccountId1 == createAccount1vs1Request.AccountId1 && x.AccountId2 == createAccount1vs1Request.AccountId2 && x.RoomOfAccountIn1vs1Id == createAccount1vs1Request.RoomOfAccountIn1vs1Id);
                if (accountIn1vs1 != null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"These two accounts have already played against this room id {createAccount1vs1Request.RoomOfAccountIn1vs1Id} together", "");
                
                accIn1vs1.EndTime = date;
                accIn1vs1.Game = c;
                accIn1vs1.AccountId1Navigation = a;
                accIn1vs1.AccountId2Navigation = a2;

                if (a.Coin < createAccount1vs1Request.Coin)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Not enough coin for this 1vs1 of account Id {a.Id}", "");
                a.Coin -= createAccount1vs1Request.Coin;
                if (a2.Coin < createAccount1vs1Request.Coin)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Not enough coin for this 1vs1 of account Id {a2.Id}", "");
                a2.Coin -= createAccount1vs1Request.Coin;
                
                if (createAccount1vs1Request.WinnerId == a.Id) 
                {
                    a.Coin += (createAccount1vs1Request.Coin * 2);
                     await GetBadge(a, "Athlete");
                }

                if (createAccount1vs1Request.WinnerId == a2.Id)
                {
                    a2.Coin += createAccount1vs1Request.Coin * 2;
                     await GetBadge(a, "Athlete");
                }
                   
                if(createAccount1vs1Request.WinnerId==0 ||createAccount1vs1Request.WinnerId ==null)
                {
                    a.Coin +=createAccount1vs1Request.Coin;
                    a2.Coin += createAccount1vs1Request.Coin;
                }
                
                await _unitOfWork.Repository<AccountIn1vs1>().CreateAsync(accIn1vs1);
                await _unitOfWork.Repository<Account>().Update(a, a.Id);
                await _unitOfWork.Repository<Account>().Update(a2, a2.Id);
                await GetBadge(a, "The Tycoon");
                await GetBadge(a2, "The Tycoon");
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountIn1vs1Response>(accIn1vs1);
                rs.GameName = c.Name;
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
        private async Task<List<Badge>> GetBadgeCompleted(Account account)
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
        private async Task GetBadge(Account account, string name)
        {
            var result = await GetBadgeCompleted(account);
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
                            ["Action"] = "/achievement",
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
        public async Task<AccountIn1vs1Response> GetAccount1vs1ById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Account 1vs1 Invalid", "");
                }
                var response = _unitOfWork.Repository<AccountIn1vs1>().GetAll().AsNoTracking().Include(x => x.AccountId1Navigation)
                    .Include(x => x.AccountId2Navigation).Include(x => x.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account 1vs1 with id {id}", "");
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
        private async Task AddToCacheAsync(int id, int coin, int gameId, string uniqueId)
        {

            var cacheKey = $"{id}+{coin}+{gameId}+{uniqueId}";
            await CacheService.Instance.AddJobAsync(cacheKey, "account1vs1");
            

        }
        public async Task<dynamic> CreateRoomPlayCountervailingWithFriend(int gameId, int accountId1,int accountId2)
        {
            try
            {
                if (gameId <= 0 || accountId1 <=0 || accountId2 <=0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information  Invalid", "");
                }
                var game = _unitOfWork.Repository<Game>().Find(u => u.Id == gameId);

                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id{gameId}", "");
                }

                var a = _unitOfWork.Repository<Account>().Find (a => a.Id == accountId1);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId1} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId1} Not Available!!!!!", "");
                }
                var a2 = _unitOfWork.Repository<Account>().Find(a => a.Id == accountId2);
                if (a2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId2} Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId2}  Not Available!!!!!", "");
                }
                var friend=_unitOfWork.Repository<Friend>().Find(x=>x.AccountId2 == accountId2 && x.AccountId1==accountId1&& x.Status==true || x.AccountId1==accountId2 && x.AccountId2==accountId1 && x.Status==true);
                if (friend == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId1} and account id {accountId2} is not friend so can not play 1vs1 together","");
                
                var uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
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
                                                           new FirebaseAdmin.Messaging.Notification() { Title = $"ThinkTank Countervailing With Friend {uniqueId}/{gameId} ", Body = $"You receive an invitation to play countervailing mode from your friend {a.FullName}", ImageUrl = a.Avatar }, data);
                #endregion
                Notification notification = new Notification
                {
                    AccountId = a2.Id,
                    Avatar = a.Avatar,
                    Account=a2,
                    DateNotification = date,
                    Description = $"You receive an invitation to play countervailing mode from your friend {a.FullName}",
                    Status = false,
                    Title = $"ThinkTank Countervailing With Friend {uniqueId}/{gameId}"
                };
                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                await _unitOfWork.CommitAsync();
                return new
                {
                    AccountId = a2.Id,
                    Avatar = a2.Avatar,
                    Coin = a2.Coin,
                    RoomId = uniqueId,
                    Username = a2.UserName
                };
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Match play countervailing mode with friend error!!!!!", ex.Message);
            }
        }
        private async Task RemoveFromCacheAsync(string accountInfo)
        {
            await CacheService.Instance.DeleteJobAsync("account1vs1",accountInfo);
        }
        public async Task<bool> RemoveAccountFromCache(int id, int coin, int gameId,string uniqueId, int delay)
        {
            try
            {
                if (id <= 0 || coin <=0 || gameId <= 0 || uniqueId == null || uniqueId == "")
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
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

                if (game == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Game Id {gameId} not found", "");
                var list = await CacheService.Instance.GetJobsAsync("account1vs1");
                string acc =  list.SingleOrDefault(x => x == $"{id}+{coin}+{gameId}+{uniqueId}");
                if (acc == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} Not Found In Cache!!!!!", "");

                Thread.Sleep(delay * 1000);
                 await RemoveFromCacheAsync( acc);
                return true;
            }
            catch (CrudException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Remove account from cache Error!!!", ex.InnerException?.Message);
            }
        }
            public async Task<dynamic> FindAccountTo1vs1(int id, int coin, int gameId)
            {
                try
                {
                        await _semaphore.WaitAsync(); // Di chuyển semaphore vào bên trong để giảm thiểu thời gian chờ đợi
                        try
                        {
                            if (id <= 0 || coin <=0 || gameId <=0)
                            {
                                throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
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
                            var accountId = 0;
                            var uniqueId = "";
                            var accountFind = new Account();
                            var list = await CacheService.Instance.GetJobsAsync("account1vs1");
                            if (list?.Any() == true)
                            {
                                foreach (var accountInfo in list)
                                {
                                    var accountValues = accountInfo.Split('+');

                                    var accountIdFromCache = Int32.Parse(accountValues[0]);
                                    var coinFromCache = Int32.Parse(accountValues[1]);
                                    var gameIdFromCache = Int32.Parse(accountValues[2]);
                                    if (coinFromCache == coin && gameIdFromCache == gameId && accountIdFromCache == id)
                                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {id} has been added to the queue", "");
                                    if (coinFromCache == coin && gameIdFromCache == gameId)
                                    {
                                        accountId = accountIdFromCache;
                                        accountFind = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);
                                        await RemoveFromCacheAsync(accountInfo); // Xóa dữ liệu khỏi cache sau khi tìm thấy tài khoản
                                        uniqueId = accountValues[3];
                                        break;
                                    }
                                }

                                if (accountId == 0)
                                {
                                    uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                                    await AddToCacheAsync(id, coin, gameId,uniqueId);
                                }
                            }
                            else
                            {
                                uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                                await AddToCacheAsync(id, coin, gameId,uniqueId);
                            }
                            return new
                            {
                                AccountId = accountId,
                                Avatar=accountFind.Avatar,
                                Coin=accountFind.Coin,
                                RoomId=uniqueId,
                                Username=accountFind.UserName
                            };
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
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


        public async Task<PagedResults<AccountIn1vs1Response>> GetAccount1vs1s(AccountIn1vs1Request request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AccountIn1vs1Response>(request);
                var account1vs1s = _unitOfWork.Repository<AccountIn1vs1>().GetAll().AsNoTracking().Include(x => x.AccountId1Navigation)
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
                        RoomOfAccountIn1vs1Id=x.RoomOfAccountIn1vs1Id,
                        Username1 = x.AccountId1Navigation.UserName,
                        Username2 = x.AccountId2Navigation.UserName,
                        WinnerId = x.WinnerId
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AccountIn1vs1Response>.Sorting(paging.SortType, account1vs1s, paging.ColName);
                var result = PageHelper<AccountIn1vs1Response>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get accounts 1vs1 list error!!!!!", ex.Message);
            }
        }
        public async Task<bool> GetToStartRoom(string room1vs1Id, bool isUser1, int time, int progressTime)
        {
            try
            {
                if (room1vs1Id == null || room1vs1Id =="" || isUser1==null || time<0 || progressTime <0 )
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid", "");
                }

                var roomRealtimeDatabase = await _firebaseRealtimeDatabaseService.GetAsyncOfRoom<dynamic>($"battle/{room1vs1Id}");
                Thread.Sleep(time * 1000 + 15000);
                await _unitOfWork.CommitAsync();
                if (roomRealtimeDatabase != null)
                {
                    if (isUser1 == true)
                        await _firebaseRealtimeDatabaseService.SetAsyncOfRoom<int>($"battle/{room1vs1Id}/progress1", progressTime);
                    else  await _firebaseRealtimeDatabaseService.SetAsyncOfRoom<int>($"battle/{room1vs1Id}/progress2", progressTime);
                }             
                return true ;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Start  To Play 1vs1 error!!!!!", ex.Message);
            }
        }
    }
}
