using AutoMapper;
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

namespace ThinkTank.Service.Services.ImpService
{
    public class ContestService : IContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        private readonly ICacheService _cacheService;
        public ContestService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
            _cacheService = cacheService;
        }

        public async Task<ContestResponse> CreateContest(CreateAndUpdateContestRequest createContestRequest)
        {
            try
            {
                var contest = _mapper.Map<CreateAndUpdateContestRequest, Contest>(createContestRequest);
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
                contest.Name = createContestRequest.Name;
                contest.Thumbnail = createContestRequest.Thumbnail;
                contest.StartTime = createContestRequest.StartTime;
                contest.EndTime = createContestRequest.EndTime;
                contest.CoinBetting = createContestRequest.CoinBetting;
                contest.GameId = createContestRequest.GameId;
                if (contest.StartTime > contest.EndTime || createContestRequest.StartTime < DateTime.Now || createContestRequest.EndTime < DateTime.Now)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                }
                contest.Status = null;
                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();
                foreach (var type in createContestRequest.Assets)
                {
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
                await _unitOfWork.Repository<Contest>().CreateAsync(contest);
                await _unitOfWork.CommitAsync();

                var expiryTime = contest.EndTime;
                var id = BackgroundJob.Schedule(() =>
                  SendNotification(contest.Id),
                 contest.StartTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdStartTime", id, expiryTime);
                var endId = BackgroundJob.Schedule(() =>
                  UpdateStateContest(contest.Id),
                          contest.EndTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdEndTime", endId, expiryTime);
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

        public async Task SendNotification(int id)
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
                        DateTime = DateTime.Now,
                        Description = $"\"{contest.Name}\"” is opened. Join now.",
                        Titile = "ThinkTank Contest"
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
        public async Task<ContestResponse> UpdateStateContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>().GetAll().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                if (contest.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Contest {id.ToString()} has ended", "");
                contest.Status = false;

                await _unitOfWork.Repository<Contest>().Update(contest, id);
                #region send noti for account
                var accounts = _unitOfWork.Repository<AccountInContest>().GetAll().Include(a => a.Account).Where(a => a.ContestId == contest.Id).ToList();
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
                _firebaseMessagingService.SendToTopic($"/topics/contestId{contest.Id}",
                                                       new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Contest", Body = $"\"{contest.Name}\"” is closed. You have received.", ImageUrl = $"{contest.Thumbnail}" }, data);
                if (accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        Notification notification = new Notification
                        {
                            AccountId = account.AccountId,
                            Avatar = contest.Thumbnail,
                            DateTime = DateTime.Now,
                            Description = $"\"{contest.Name}\"” is closed. You have received {account.Prize}.",
                            Titile = "ThinkTank Contest"
                        };

                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                        #endregion
                    }
                }
                var fcmTokens = accounts.Where(a => a.Account.Fcm != null && a.Account.Fcm != "").Select(a => a.Account.Fcm).ToList();
                if (fcmTokens.Any())
                    _firebaseMessagingService.Unsubcribe(fcmTokens, $"/topics/contestId{contest.Id}");
                if (contest.EndTime > DateTime.Now)
                {
                    var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                    if (jobId != null)
                    {
                        BackgroundJob.Delete(jobId);
                        _cacheService.RemoveData(jobId);
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
                    account.Coin += reward;
                    await _unitOfWork.Repository<Account>().Update(account, account.Id);
                    if (account.Coin == 4000)
                        GetBadge(account, "The Tycoon");
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
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                    badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone)
                {
                    badge.Status = true;
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
                        Titile = "ThinkTank"
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
                await _unitOfWork.Repository<Badge>().CreateAsync(b);
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
                var response = _unitOfWork.Repository<Contest>().GetAll().Include(x=>x.Game).Include(x=>x.AccountInContests).SingleOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {id.ToString()}", "");
                }

                var rs= _mapper.Map<ContestResponse>(response);
                rs.AmoutPlayer = response.AccountInContests.Count();
                rs.GameName = response.Game.Name;
                return rs;
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
        public async Task<List<LeaderboardResponse>> GetLeaderboardOfContest(int id)
        {
            try
            {
                var contest = _unitOfWork.Repository<Contest>().GetAll().Include(c => c.AccountInContests)
                      .SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                if (contest.AccountInContests.Count() > 0)
                {
                    var orderedAccounts = contest.AccountInContests.OrderByDescending(x => x.Mark).ThenBy(x => x.Duration);
                    var rank = 1;

                    foreach (var account in orderedAccounts)
                    {
                        var leaderboardContestResponse = new LeaderboardResponse
                        {
                            AccountId = account.AccountId,
                            Mark = account.Mark
                        };

                        var mark = contest.AccountInContests
                            .Where(x => x.Mark == account.Mark && x.Duration == account.Duration && x.AccountId != account.AccountId)
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

        public async Task<PagedResults<ContestResponse>> GetContests(ContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request);
                var contests = _unitOfWork.Repository<Contest>().GetAll().Include(x => x.AssetOfContests).Include(x=>x.Game)
                                           .Select(x=>new ContestResponse
                                           {
                                               Id = x.Id,
                                               EndTime=x.EndTime,
                                               StartTime=x.StartTime,
                                               AssetOfContests=_mapper.Map<List<AssetOfContestResponse>>(x.AssetOfContests),
                                               Name=x.Name,
                                               Status=x.Status,
                                               Thumbnail=x.Thumbnail,
                                               GameId=x.GameId,
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
                Contest contest = _unitOfWork.Repository<Contest>()
                     .GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == contestId);

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
                if (contest.StartTime <= DateTime.Now)
                    throw new CrudException(HttpStatusCode.BadRequest, "The contest has already started and you cannot update it", "");
                var job = _cacheService.GetData<string>($"Contest{contest.Id}JobIdStartTime");
                if (job != null)
                    BackgroundJob.Delete(job);
                var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                if (jobId != null)
                    BackgroundJob.Delete(jobId);
                _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
                List<AssetOfContestResponse> result = new List<AssetOfContestResponse>();
                List<AssetOfContest> list = new List<AssetOfContest>();
                foreach (var type in request.Assets)
                {
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
                _mapper.Map<CreateAndUpdateContestRequest, Contest>(request, contest);
                contest.Id = contestId;
                await _unitOfWork.Repository<Contest>().Update(contest, contestId);
                await _unitOfWork.CommitAsync();
                var expiryTime = contest.EndTime;
                var id = BackgroundJob.Schedule(() =>
                  SendNotification(contest.Id),
                 contest.StartTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdStartTime", id, expiryTime);
                var endId = BackgroundJob.Schedule(() =>
                    UpdateStateContest(contest.Id),
                            contest.EndTime.Subtract(DateTime.Now));
                _cacheService.SetData<string>($"Contest{contest.Id}JobIdEndTime", endId, expiryTime);
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
                Contest contest = _unitOfWork.Repository<Contest>()
                      .GetAll().Include(x => x.AssetOfContests).SingleOrDefault(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                _unitOfWork.Repository<AssetOfContest>().DeleteRange(contest.AssetOfContests.ToArray());
                await _unitOfWork.Repository<Contest>().RemoveAsync(contest);
                await _unitOfWork.CommitAsync();
                var job = _cacheService.GetData<string>($"Contest{contest.Id}JobIdStartTime");
                if (job != null)
                {
                    BackgroundJob.Delete(job);
                    _cacheService.RemoveData(job);
                }
                var jobId = _cacheService.GetData<string>($"Contest{contest.Id}JobIdEndTime");
                if (jobId != null)
                {
                    BackgroundJob.Delete(jobId);
                    _cacheService.RemoveData(jobId);
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
                .Include(x => x.AccountInContests).Include(x=>x.Game)
                .Where(x => x.StartTime.Month == DateTime.Now.Month)
                .OrderByDescending(x => x.AccountInContests.Count())
                 .ToList();

                var bestContest = contests.FirstOrDefault();

                double percentAverageScore = 0;

                if (bestContest != null)
                {
                    var highScoreOfContest = bestContest.AccountInContests.Max(x => x.Mark);
                    var lowestScoreOfContest = bestContest.AccountInContests.Min(x => x.Mark);
                    var averageScore = (highScoreOfContest + lowestScoreOfContest) / 2;

                    var usersInAverageScore = bestContest.AccountInContests
                        .Count(x => x.Mark > averageScore - 100 && x.Mark < averageScore + 100);

                    var totalUserInContest = bestContest.AccountInContests.Count();
                    percentAverageScore = totalUserInContest > 0 ? (double)usersInAverageScore / totalUserInContest * 100 : 0;
                }

                return new
                {
                    NameTopContest=bestContest.Name,
                    PercentAverageScore = percentAverageScore,
                    Contests = contests.Select(x => new ContestResponse
                    {
                        Id = x.Id,
                        EndTime = x.EndTime,
                        StartTime = x.StartTime,
                        Name = x.Name,
                        Status = x.Status,
                        Thumbnail = x.Thumbnail,
                        GameId = x.GameId,
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

