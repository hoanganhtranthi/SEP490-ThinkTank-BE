using AutoMapper;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
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
using Hangfire;

namespace ThinkTank.Service.Services.ImpService
{
    public class AchievementService : IAchievementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        public AchievementService(IUnitOfWork unitOfWork, IMapper mapper,IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<AchievementResponse> CreateAchievement(CreateAchievementRequest createAchievementRequest)
        {
            try
            {
                var achievement = _mapper.Map<CreateAchievementRequest, Achievement>(createAchievementRequest);
                var game = _unitOfWork.Repository<Game>().GetAll().SingleOrDefault(x => x.Id == createAchievementRequest.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This game id {createAchievementRequest.GameId} is not found !!!", "");

                var account = _unitOfWork.Repository<Account>().GetAll().Include(x=>x.Achievements).SingleOrDefault(x => x.Id == createAchievementRequest.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account id {createAchievementRequest.AccountId} is not found !!!", "");

                var levels = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.AccountId == createAchievementRequest.AccountId && createAchievementRequest.GameId == x.GameId).OrderBy(x => x.Level).ToList();
                var level = 0;
                if (levels.Count() > 0)
                    level = levels.LastOrDefault().Level;
                else level = 0;
                if (createAchievementRequest.Level > level + 1 || createAchievementRequest.Level <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Level", "");

                achievement.CompletedTime = DateTime.Now;
                var highScore = account.Achievements.Where(x => x.AccountId == account.Id && x.Level == achievement.Level && x.GameId == game.Id).OrderByDescending(x => x.Mark).FirstOrDefault();
                if (highScore!= null && createAchievementRequest.Mark > highScore.Mark)
                    GetBadge(account, "The Breaker");
                await _unitOfWork.Repository<Achievement>().CreateAsync(achievement);
                var leaderboard = GetLeaderboard(createAchievementRequest.GameId).Result;
                var top1 = leaderboard.FirstOrDefault();
                var topAccountId = top1?.AccountId;

                if (createAchievementRequest.Duration < 20)
                    GetBadge(account, "Fast and Furious");

                var acc = leaderboard.SingleOrDefault(x => x.AccountId == account.Id);
                if ( leaderboard.Count()>1 &&(account.Achievements.Any(x => x.GameId == createAchievementRequest.GameId && x.Level == createAchievementRequest.Level) &&
                    (acc != null && acc.Mark + createAchievementRequest.Mark >= top1?.Mark)))
                {
                    GetBadge(account, "Legend");
                }
                var list = new List<Achievement>();
                foreach (var result in account.Achievements)
                {
                    var t = _unitOfWork.Repository<Game>().GetAll().SingleOrDefault(x => x.Id == result.GameId);
                    if (list.SingleOrDefault(x => x.GameId == t.Id) == null)
                    {
                        if (result.Level == 10)
                            list.Add(result);
                    }

                }
                
                var twoLastAchievement = account.Achievements.Where(x => x.GameId == game.Id).Skip(Math.Max(0, account.Achievements.Count - 2)).Take(2).ToList();
                if (twoLastAchievement.Any())
                {
                    bool areConsecutive = twoLastAchievement.Count() == 2 && twoLastAchievement.ToArray()[0].Level == twoLastAchievement.ToArray()[1].Level - 1 && twoLastAchievement.ToArray()[0].Mark > 0 && twoLastAchievement.ToArray()[1].Mark > 0;
                    if (createAchievementRequest.Level != twoLastAchievement.LastOrDefault().Level && createAchievementRequest.Mark > 0 && areConsecutive && twoLastAchievement.Last().Level + 1 == createAchievementRequest.Level)
                    {
                        GetBadge(account, "Streak killer");
                    }
                }
                if (account.Achievements.Count(x => x.GameId == createAchievementRequest.GameId && x.Level == 10) == 1)
                {
                    GetPlowLordBadge(account, list);
                }

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AchievementResponse>(achievement);
                rs.Username = account.UserName;
                rs.GameName = game.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Achievement Error!!!", ex?.Message);
            }
        }
        private async Task GetPlowLordBadge(Account account,List<Achievement>list)
        {
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals("Plow Lord"));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals("Plow Lord"));
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Title == $"You have received {challage.Name} badge.");
            if (badge != null && list.Count() == (badge.CompletedLevel + 1))
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
                        Status = false,
                        Description = $"You have received {challage.Name} badge.",
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
        private async Task GetBadge(Account account, string name)
        {
                    var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                    var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Title == $"You have received {challage.Name} badge.");
                    if (badge != null)
                {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                    badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone && noti ==null )
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
                        Status=false,
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
        public async Task<AchievementResponse> GetAchievementById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Achievement Invalid", "");
                }
                var response = _unitOfWork.Repository<Achievement>().GetAll().Include(c=>c.Account).Include(c=>c.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found achievement with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AchievementResponse>(response);
                rs.Username=response.Account.UserName;
                rs.GameName = response.Game.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Achievement By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AchievementResponse>> GetAchievements(AchievementRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AchievementResponse>(request);
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Account).Include(x=>x.Game)
                    .Select(x => new AchievementResponse
                    {
                        Id = x.Id,
                        GameName = x.Game.Name,
                        AccountId=x.AccountId,
                        CompletedTime=x.CompletedTime,
                        Duration=x.Duration,
                      GameId=x.GameId,
                       PieceOfInformation=x.PieceOfInformation,
                        Level=x.Level,
                        Mark=x.Mark,
                        Username=x.Account.UserName
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AchievementResponse>.Sorting(paging.SortType, achievements, paging.ColName);
                var result = PageHelper<AchievementResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get achievement list error!!!!!", ex.Message);
            }
        }
        public async Task<PagedResults<LeaderboardResponse>> GetLeaderboard(int id, PagingRequest paging)
        {
            try
            {
                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == id);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {id} not found", "");
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(c => c.Account).Include(c => c.Game)
                    .Where(x => x.GameId == id).ToList();

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                List<Achievement> achievementsList = new List<Achievement>();
                if (achievements.Count() > 0)
                {
                    foreach (var achievement in achievements)
                    {
                        if (achievementsList.Count(x => x.AccountId == achievement.AccountId) == 0)
                        {
                            var rs = GetSumScoreOfAccount(achievement.AccountId, achievements);
                            if (rs != null)
                                achievementsList.Add(rs);
                        }
                    }
                    var orderedAccounts = achievementsList.OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var achievement in orderedAccounts)
                    {
                        if (responses.Count(a => a.AccountId == achievement.AccountId) == 0)
                        {
                            var leaderboardContestResponse = new LeaderboardResponse
                            {
                                AccountId = achievement.AccountId,
                                Mark = achievement.Mark,
                                Avatar = achievement.Account.Avatar,
                                FullName = achievement.Account.FullName
                            };

                            var mark = achievementsList
                                .Where(x => x.Mark == achievement.Mark && x.AccountId != achievement.AccountId)
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

                }
                return PageHelper<LeaderboardResponse>.Paging(responses.ToList(), paging.Page, paging.PageSize);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of achievement error!!!!!", ex.Message);
            }
        }
        private async Task<List<LeaderboardResponse>> GetLeaderboard(int id)
        {
            try
            {
                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == id);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {id} not found","");
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(c => c.Account).Include(c => c.Game)
                    .Where(x=>x.GameId==id ).ToList();

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                List<Achievement> achievementsList = new List<Achievement>();
                if (achievements.Count() > 0)
                {                  
                    foreach(var achievement in achievements)
                    {
                        if(achievementsList.Count(x=>x.AccountId ==achievement.AccountId)==0)
                        {
                            var rs = GetSumScoreOfAccount(achievement.AccountId, achievements);
                            if (rs != null)
                                achievementsList.Add(rs);
                        }
                    }
                    var orderedAccounts = achievementsList.OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var achievement in orderedAccounts)
                    {
                        if (responses.Count(a => a.AccountId == achievement.AccountId)==0)
                        {
                                var leaderboardContestResponse = new LeaderboardResponse
                                {
                                    AccountId = achievement.AccountId,
                                    Mark = achievement.Mark,
                                    Avatar = achievement.Account.Avatar,
                                    FullName = achievement.Account.FullName
                                };

                                var mark = achievementsList
                                    .Where(x => x.Mark == achievement.Mark && x.AccountId != achievement.AccountId)
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
        private Achievement GetSumScoreOfAccount(int id, List<Achievement> achievements)
        {
            List<Achievement> responses = new List<Achievement>();
            var score = 0;
            Account account = null;
            foreach (var achievement in achievements)
            {
                if (responses.Count(a => a.Level == achievement.Level)==0)
                {
                    var highestScore = achievements.Where(x => x.AccountId == id && x.Level == achievement.Level).OrderByDescending(x=>x.Mark).FirstOrDefault();
                    if (highestScore != null)
                    {
                        score += highestScore.Mark;
                        responses.Add(highestScore);
                        account = highestScore.Account;
                    }
                }
            }
            return new Achievement
            {
                AccountId = id,
                Mark = responses.Sum(x => x.Mark),
                Account = account
            };
        }
    }
}
