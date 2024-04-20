﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using Repository.Extensions;
using OpenAI_API.Moderation;
using System.Runtime.ConstrainedExecution;
using static Google.Apis.Requests.BatchRequest;
using FirebaseAdmin.Messaging;
using Notification = ThinkTank.Data.Entities.Notification;
using System.Net.NetworkInformation;

namespace ThinkTank.Service.Services.ImpService
{
    public class ContestService : IContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService;
        public ContestService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService, IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            this.firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        }

        public async Task<ContestResponse> CreateContest(CreateAndUpdateContestRequest createContestRequest)
        {
            try
            {
                if (createContestRequest.Name == null || createContestRequest.Name == "" || createContestRequest.Thumbnail == null || createContestRequest.Thumbnail=="" 
                    || createContestRequest.StartTime==null || createContestRequest.EndTime ==null)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var contest = _mapper.Map<CreateAndUpdateContestRequest, Contest>(createContestRequest);

                if(createContestRequest.Assets ==null || createContestRequest.Assets.Count()==0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Assets Of Contest cannot be null", "");

                var s = _unitOfWork.Repository<Contest>().Find(s => s.Name == createContestRequest.Name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest has already !!!", "");
                }
                var game = _unitOfWork.Repository<Game>().Find(s => s.Id == createContestRequest.GameId);
                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Game {createContestRequest.GameId} not found !!!", "");
                }
                if (contest.StartTime > contest.EndTime || createContestRequest.StartTime < date || createContestRequest.EndTime < date)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                }

                contest.Name = createContestRequest.Name;
                contest.Thumbnail = createContestRequest.Thumbnail;
                contest.StartTime = createContestRequest.StartTime;
                contest.EndTime = createContestRequest.EndTime;
                contest.CoinBetting = createContestRequest.CoinBetting;
                contest.GameId = createContestRequest.GameId;              
                contest.Status = null;

                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();

                if (game.Name.Equals("Flip Card"))
                {
                    if (!(createContestRequest.Assets.Count()==3 || createContestRequest.Assets.Count()==4 || createContestRequest.Assets.Count()==6
                         || createContestRequest.Assets.Count()==8 || createContestRequest.Assets.Count()==10 || createContestRequest.Assets.Count()==12
                         || createContestRequest.Assets.Count()==14))
                    {
                        throw new CrudException(HttpStatusCode.NotFound, "The number of assets for the flip card game must be 3,4,6,8,10,12 or 14", "");
                    }
                }

                foreach (var type in createContestRequest.Assets)
                {
                    if(type.Value ==null || type.Value == "" || type.TypeOfAssetId <=0)
                            throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _mapper.Map<CreateAssetOfContestRequest, AssetOfContest>(type);
                    var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == type.TypeOfAssetId);
                    if (t == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Not Found!!!!!", "");
                    }

                    if (game.Name.Equals("Flip Card") || game.Name.Equals("Images Walkthrough"))
                    {
                        if (t.Type.Equals("Description+ImgLink") || t.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (asset.Value.Contains(";") || asset.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }
                    else if (game.Name.Equals("Music Password"))
                    {
                        if (t.Type.Equals("Description+ImgLink") || t.Type.Equals("ImgLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!asset.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }
                    else
                    {
                        if (t.Type.Equals("ImgLink") || t.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset In Contest Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!asset.Value.Contains(";"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset In Contest Invalid!!!!!", "");
                            }
                        }
                    }

                    var existingAsset = _unitOfWork.Repository<AssetOfContest>().Find(s => s.Value.Equals(type.Value));
                    if (existingAsset != null)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                    }

                    AssetOfContest assetOfContest = new AssetOfContest();
                    assetOfContest.Value = type.Value;
                    assetOfContest.TypeOfAssetId = type.TypeOfAssetId;
                    assetOfContest.ContestId = contest.Id;
                    assetOfContest.Contest = contest;
                    AssetOfContestResponse response = new AssetOfContestResponse();
                    response.Id = assetOfContest.Id;
                    response.Value = assetOfContest.Value;
                    response.NameOfContest = contest.Name;
                    response.Type = t.Type;
                    list.Add(assetOfContest);
                    contest.AssetOfContests = list;
                    result.Add(response);
                }

                await _unitOfWork.Repository<Contest>().CreateAsync(contest);
                await _unitOfWork.CommitAsync();
                var id = BackgroundJob.Schedule(() =>
                  SendNotification(contest.Id),
                 contest.StartTime.Subtract(date));
               await firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdStartTime", id);
                var endId = BackgroundJob.Schedule(() =>
                  UpdateStateContest(contest.Id),
                          contest.EndTime.Subtract(date));
               await firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdEndTime", endId);

                return _mapper.Map<ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Contest Error!!!", ex?.Message);
            }
        }

        private async Task SendNotification(int id)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == id);
                #region send noti for account
                var accounts = _unitOfWork.Repository<Account>().GetAll().Where(a => a.Status == true).ToList();
                var fcmTokens = accounts.Where(a => a.Fcm != null && a.Fcm != "").Select(a => a.Fcm).ToList();
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
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Contest", Body = $"\"{contest.Name}\"” is opened. Join now.", ImageUrl = $"{contest.Thumbnail}" }, data);
                #endregion
                contest.Status = true;
                await _unitOfWork.Repository<Contest>().Update(contest, contest.Id);
                foreach (var account in accounts)
                {
                    Notification notification = new Notification
                    {
                        AccountId = account.Id,
                        Avatar = contest.Thumbnail,
                        DateNotification = date,
                        Status=false,
                        Description = $"\"{contest.Name}\"” is opened. Join now.",
                        Title = "ThinkTank Contest"
                    };

                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
                await _unitOfWork.CommitAsync();
            }
            catch (CrudException ex)
            {
                throw ex;
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
        private async Task<ContestResponse> UpdateStateContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>().GetAll().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id}", "");
                }
                if (contest.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Contest {id} has ended", "");
                contest.Status = false;
                var jobId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdEndTime").Result;
                if (jobId != null)
                {                   
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }
                var jobStartId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdStartTime").Result;
                if (jobStartId != null)
                {
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdStartTime");
                }
                if (date < contest.StartTime)
                    BackgroundJob.Delete(jobStartId);
                if (date < contest.EndTime)
                    BackgroundJob.Delete(jobId);
                await _unitOfWork.Repository<Contest>().Update(contest, id);
                #region send noti for account
                var accounts = _unitOfWork.Repository<AccountInContest>().GetAll().Include(a => a.Account).Where(a => a.ContestId == contest.Id && a.Account.Status==true).ToList();
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
                List<string> fcmTokens = new List<string>();
                fcmTokens=accounts.Where(x=>x.Account.Fcm !=null).Select(x=>x.Account.Fcm).ToList();
                if (fcmTokens.Any())                   
                    _firebaseMessagingService.SendToDevices(fcmTokens,
                                                       new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Contest", Body = $"\"{contest.Name}\"” is closed. Thank you for participating in the contest.", ImageUrl = $"{contest.Thumbnail}" }, data);
                if (accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        Notification notification = new Notification
                        {
                            AccountId = account.AccountId,
                            Avatar = contest.Thumbnail,
                            DateNotification= date,
                            Description = $"\"{contest.Name}\"” is closed.Thank you for participating in the contest.",
                            Status = false,
                            Title = "ThinkTank Contest"
                        };

                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                        #endregion
                    }
                }                 
                var leaderboard = await GetLeaderboardOfContest(id);

                foreach (var contestant in leaderboard.Take(3))
                {
                    var account = await _unitOfWork.Repository<Account>().FindAsync(x => x.Id == contestant.AccountId);

                    int rewardPercentage = 0;
                    switch (contestant.Rank)
                    {
                        case 1:
                            rewardPercentage = 50;
                            break;
                        case 2:
                            rewardPercentage = 30;
                            break;
                        case 3:
                            rewardPercentage = 20;
                            break;
                    }
                    int reward = (int)Math.Round((decimal)(contest.CoinBetting * contest.AccountInContests.Count * (rewardPercentage / 100.0m)));
                    account.Coin =account.Coin+reward;
                    await _unitOfWork.Repository<Account>().Update(account, account.Id);
                    fcmTokens.Clear();
                    if (account.Status == true)
                    {
                        if (account.Fcm != null)
                            fcmTokens.Add(account.Fcm);
                        if (fcmTokens.Any())
                            _firebaseMessagingService.SendToDevices(fcmTokens,
                          new FirebaseAdmin.Messaging.Notification()
                          {
                              Title = "ThinkTank Contest",
                              Body = $"Congratulations! You won top 1 in the contest \"{contest.Name}\" and received {reward} ThinkTank coins”",
                              ImageUrl = $"{contest.Thumbnail}"
                          }, data);
                        Notification notification = new Notification
                        {
                            AccountId = account.Id,
                            Avatar = contest.Thumbnail,
                            DateNotification = date,
                            Description = $"Congratulations! You won top 1 in the contest \"{contest.Name}\" and received {reward} ThinkTank coins”",
                            Status = false,
                            Title = "ThinkTank Contest"
                        };

                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                        await GetBadge(account, "The Tycoon");
                    }
                }

                await _unitOfWork.CommitAsync();
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status's contest error!!!!!", ex.Message);
            }
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
                    if (badge.CompletedLevel < challage.CompletedMilestone && account.Coin < challage.CompletedMilestone)
                        badge.CompletedLevel = (int)account.Coin;
                    if (account.Coin >= challage.CompletedMilestone)
                    {
                        badge.Status = true;
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
            }
        }
        public async Task<ContestResponse> GetContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Contest Invalid", "");
                }
                var response = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(x=>x.Game).Include(x=>x.AccountInContests)
                                           .Select(x => new ContestResponse
                                           {
                                               Id = x.Id,
                                               EndTime = x.EndTime,
                                               StartTime = x.StartTime,
                                               AssetOfContests = _mapper.Map<List<AssetOfContestResponse>>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                                               {
                                                   ContestId = a.ContestId,
                                                   Id = a.Id,
                                                   Value = a.Value,
                                                   NameOfContest = x.Name,
                                                   Answer = x.GameId == 2 ? System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).LastIndexOf('.')) : null,
                                                   Type = a.TypeOfAsset.Type
                                               })),
                                               Name = x.Name,
                                               Status = x.Status,
                                               Thumbnail = x.Thumbnail,
                                               GameId = x.GameId,
                                               PlayTime = x.PlayTime,
                                               CoinBetting = x.CoinBetting,
                                               GameName = x.Game.Name,
                                               AmoutPlayer = x.AccountInContests.Count()
                                           }).SingleOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }
        private async Task<List<LeaderboardResponse>> GetLeaderboardOfContest(int contestId)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == contestId);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{contestId.ToString()}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (contest.AccountInContests.Count() > 0)
                {
                    var orderedAccounts = contest.AccountInContests.Where(x=>x.Mark != 0).OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var account in orderedAccounts)
                    {
                        var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == account.AccountId);
                        var leaderboardContestResponse = new LeaderboardResponse
                        {
                            AccountId = account.AccountId,
                            Mark = account.Mark,
                            Avatar=acc.Avatar,
                            FullName=acc.FullName
                        };

                        var mark = contest.AccountInContests
                            .Where(x => x.Mark == account.Mark && x.AccountId != account.AccountId)
                            .ToList();

                        if (mark.Any())
                        {
                            var a = responses.SingleOrDefault(a => a.AccountId == mark.First().AccountId);
                            leaderboardContestResponse.Rank = a?.Rank ?? rank;// a != null: leaderboardContestResponse.Rank = a.Rank va nguoc lai a==null : leaderboardContestResponse.Rank = rank
                        }
                        else
                        {
                            leaderboardContestResponse.Rank = rank;
                        }

                        responses.Add(leaderboardContestResponse);
                        rank++;
                    }

                }
                return responses.ToList();
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of contest error!!!!!", ex.Message);
            }
        }
        public async Task<PagedResults<LeaderboardResponse>> GetLeaderboardOfContest(int contestId, PagingRequest paging)
        {
            try
            {
                if (contestId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var contest = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == contestId);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{contestId.ToString()}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (contest.AccountInContests.Count() > 0)
                {
                    var orderedAccounts = contest.AccountInContests.OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var account in orderedAccounts)
                    {
                        var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == account.AccountId);
                        var leaderboardContestResponse = new LeaderboardResponse
                        {
                            AccountId = account.AccountId,
                            Mark = account.Mark,
                            Avatar = acc.Avatar,
                            FullName = acc.FullName
                        };

                        var mark = contest.AccountInContests
                            .Where(x => x.Mark == account.Mark && x.AccountId != account.AccountId)
                            .ToList();

                        if (mark.Any())
                        {
                            var a = responses.SingleOrDefault(a => a.AccountId == mark.First().AccountId);
                            leaderboardContestResponse.Rank = a?.Rank ?? rank;// a != null: leaderboardContestResponse.Rank = a.Rank va nguoc lai a==null : leaderboardContestResponse.Rank = rank
                        }
                        else
                        {
                            leaderboardContestResponse.Rank = rank;
                        }

                        responses.Add(leaderboardContestResponse);
                        rank++;
                    }

                }
                return PageHelper<LeaderboardResponse>.Paging(responses.ToList(), paging.Page, paging.PageSize);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of contest error!!!!!", ex.Message);
            }
        }
        public async Task<PagedResults<ContestResponse>> GetContests(ContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request);
                var contests = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(x => x.AssetOfContests).Include(x=>x.Game)
                                           .Select(x=>new ContestResponse
                                           {
                                               Id = x.Id,
                                               EndTime=x.EndTime,
                                               StartTime=x.StartTime,
                                               AssetOfContests=_mapper.Map<List<AssetOfContestResponse>>(x.AssetOfContests.Select(a=>new AssetOfContestResponse
                                               {
                                                   ContestId=a.ContestId,
                                                   Id=a.Id,
                                                   Value=a.Value,
                                                   NameOfContest=x.Name,
                                                   Answer = x.GameId == 2 ? System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).LastIndexOf('.')) : null,
                                                   Type = a.TypeOfAsset.Type
                                               })),
                                               Name=x.Name,
                                               Status=x.Status,
                                               Thumbnail=x.Thumbnail,
                                               GameId=x.GameId,
                                               PlayTime=x.PlayTime,
                                               CoinBetting=x.CoinBetting,
                                               GameName=x.Game.Name,
                                               AmoutPlayer=x.AccountInContests.Count()
                                           })
                                           .DynamicFilter(filter)
                                           .ToList();
                if (request.ContestStatus != Helpers.Enum.StatusType.All)
                {
                    bool? status = null;
                    if (request.ContestStatus.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.ContestStatus.ToString().ToLower());
                    }
                    contests = contests.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<ContestResponse>.Sorting(paging.SortType, contests, paging.ColName);
                var result = PageHelper<ContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get contest list error!!!!!", ex.Message);
            }
        }
        public async Task<ContestResponse> UpdateContest(int contestId, CreateAndUpdateContestRequest request)
        {
            try
            {
                if (contestId <= 0 || request.Name == null || request.Name == "" || request.Thumbnail == null || request.Thumbnail == ""
                    || request.StartTime == null || request.EndTime == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                Contest contest = _unitOfWork.Repository<Contest>()
                     .GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == contestId);
                if (request.Assets == null || request.Assets.Count() == 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Assets Of Contest cannot be null", "");
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {contestId.ToString()}", "");
                var game = _unitOfWork.Repository<Game>().Find(s => s.Id == request.GameId);
                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Game {request.GameId} not found !!!", "");
                }
                var existingContest = _unitOfWork.Repository<Contest>().GetAll().FirstOrDefault(c => c.Name.Equals(request.Name) && c.Id != contestId);
                if (existingContest != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Name of contest has already been taken", "");
                if (request.StartTime > request.EndTime)
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                if (contest.StartTime <= date)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot update it", "");
                var job = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdStartTime").Result;
                if (job != null)
                {
                    BackgroundJob.Delete(job);
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdStartTime");
                }
                var jobId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdEndTime").Result;
                if (jobId != null)
                {
                    BackgroundJob.Delete(jobId);
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }
              await  _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();
                foreach (var type in request.Assets)
                {
                    if (type.Value == null || type.Value == "" || type.TypeOfAssetId <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _mapper.Map<CreateAssetOfContestRequest, AssetOfContest>(type);
                    var t = _unitOfWork.Repository<TypeOfAssetInContest>().Find(t => t.Id == type.TypeOfAssetId);
                    if (t == null)
                    {
                        throw new CrudException(HttpStatusCode.InternalServerError, "Type Of Asset In Contest Not Found!!!!!", "");
                    }

                    AssetOfContest assetOfContest = new AssetOfContest();
                    assetOfContest.Value = type.Value;
                    assetOfContest.TypeOfAssetId = type.TypeOfAssetId;
                    assetOfContest.ContestId = contest.Id;
                    await _unitOfWork.Repository<AssetOfContest>().CreateAsync(assetOfContest);
                    AssetOfContestResponse response = new AssetOfContestResponse();
                    response.Id = assetOfContest.Id;
                    response.Value = assetOfContest.Value;
                    response.NameOfContest = contest.Name;
                    response.Type = t.Type;
                    list.Add(assetOfContest);
                    contest.AssetOfContests = list;
                    result.Add(response);
                }

                contest.Thumbnail = request.Thumbnail != null && request.Thumbnail != "" ? request.Thumbnail : contest.Thumbnail;
                _mapper.Map<CreateAndUpdateContestRequest, Contest>(request, contest);
                contest.Id = contestId;

                await _unitOfWork.Repository<Contest>().Update(contest, contestId);
                await _unitOfWork.CommitAsync();
                var id = BackgroundJob.Schedule(() =>
                  SendNotification(contest.Id),
                 contest.StartTime.Subtract(date));
               await firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdStartTime", id);
                var endId = BackgroundJob.Schedule(() =>
                    UpdateStateContest(contest.Id),
                            contest.EndTime.Subtract(date));
              await  firebaseRealtimeDatabaseService.SetAsync<string>($"Contest{contest.Id}JobIdEndTime", endId);
                
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update contest error!!!!!", ex.Message);
            }
        }
        public async Task<ContestResponse> DeleteContest(int id)
        {
            try
            {
                if (id <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                Contest contest = _unitOfWork.Repository<Contest>()
                      .GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                if (contest.StartTime <= DateTime.Now)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot delete it", "");
               await _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
                await _unitOfWork.Repository<Contest>().RemoveAsync(contest);
                await _unitOfWork.CommitAsync();
                var job =firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdStartTime").Result;
                if (job != null)
                {
                    BackgroundJob.Delete(job);
                  await  firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdStartTime");
                }
                var jobId = firebaseRealtimeDatabaseService.GetAsync<string>($"Contest{contest.Id}JobIdEndTime").Result;
                if (jobId != null)
                {
                    BackgroundJob.Delete(jobId);
                    await firebaseRealtimeDatabaseService.RemoveData($"Contest{contest.Id}JobIdEndTime");
                }
                return _mapper.Map<ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete contest error!!!!!", ex.Message);
            }
        }
        public async Task<dynamic> GetReportOfContest()
        {
            try
            {
                var contests = _unitOfWork.Repository<Contest>()
                .GetAll()
                .Include(x => x.AccountInContests).Include(x=>x.Game).AsNoTracking()
                .Where(x => x.StartTime.Month == date.Month)
                .OrderByDescending(x => x.AccountInContests.Count())
                 .ToList();
                var listBestContest=contests.Where(x => x.AccountInContests.Count() == contests.FirstOrDefault().AccountInContests.Count()).ToList();
                var resultPercentAverageScore = new Dictionary<int,double>();
                foreach (var best in listBestContest)
                {
                    if (best != null)
                    {
                        var highScoreOfContest = best.AccountInContests.Any() ? best.AccountInContests.Max(x => x.Mark) : 0;
                        var lowestScoreOfContest = best.AccountInContests.Any() ? best.AccountInContests.Min(x => x.Mark) : 0;
                        var averageScore = (highScoreOfContest + lowestScoreOfContest) / 2;

                        var usersInAverageScore = best.AccountInContests.Any() ? best.AccountInContests
                            .Count(x => x.Mark > averageScore - 100 && x.Mark < averageScore + 100) : 0;

                        var totalUserInContest = best.AccountInContests.Count();
                       var  percentAverageScore = totalUserInContest > 0 ? (double)usersInAverageScore / totalUserInContest * 100 : 0;
                        resultPercentAverageScore.Add(best.Id,percentAverageScore);
                    }
                }
                return new
                {
                    BestContestes=listBestContest.Select(x=>new
                    {
                        NameTopContest = x.Name,
                        PercentAverageScore = resultPercentAverageScore.SingleOrDefault(a=>a.Key==x.Id).Value,
                    }) ,                   
                    Contests = contests.Select(x => new ContestResponse
                    {
                        Id = x.Id,
                        EndTime = x.EndTime,
                        StartTime = x.StartTime,
                        Name = x.Name,
                        Status = x.Status,
                        Thumbnail = x.Thumbnail,
                        GameId = x.GameId,
                        PlayTime = x.PlayTime,
                        GameName = x.Game.Name,
                        AmoutPlayer = x.AccountInContests.Count()
                    }),
                };

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Report Of Contest error!!!!!", ex.Message);
            }
        }
    }
}

