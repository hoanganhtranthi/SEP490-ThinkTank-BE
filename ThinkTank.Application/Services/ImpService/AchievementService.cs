using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Services.ImpService
{
    public class AchievementService : IAchievementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
      
        public AchievementService(IUnitOfWork unitOfWork, IMapper mapper,IFirebaseMessagingService firebaseMessagingService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<AchievementResponse> CreateAchievement(CreateAchievementRequest createAchievementRequest)
        {
            try
            {
                if (createAchievementRequest.AccountId <= 0 || createAchievementRequest.Duration < 0 || createAchievementRequest.Level <= 0 || createAchievementRequest.Mark < 0
                    || createAchievementRequest.PieceOfInformation <= 0 || createAchievementRequest.GameId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var achievement = _mapper.Map<CreateAchievementRequest, Achievement>(createAchievementRequest);

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == createAchievementRequest.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This game id {createAchievementRequest.GameId} is not found !!!", "");

                var account = _unitOfWork.Repository<Account>().GetAll().Include(x=>x.Achievements).SingleOrDefault(x => x.Id == createAchievementRequest.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account id {createAchievementRequest.AccountId} is not found !!!", "");

                if (account.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {account.Id} not available!!!!!", "");

                //Check level có đúng hay không (không lớn hơn level hiện tại +1)
                var levels = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.AccountId == createAchievementRequest.AccountId && createAchievementRequest.GameId == x.GameId).OrderBy(x => x.Level).ToList();
                var level = 0;
                if (levels.Count() > 0)
                    level = levels.LastOrDefault().Level;
                else level = 0;
                if (createAchievementRequest.Level > level + 1 || createAchievementRequest.Level <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Level", "");

                achievement.Account = account;
                achievement.Game = game;
                achievement.CompletedTime = date;

                #region Streak killer badge
                var gameAchievement = account.Achievements.Where(x => x.GameId == game.Id).ToList();
                var badge= _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.Challenge.Name == "Streak killer" && x.AccountId == account.Id);
                var number = badge != null && gameAchievement.Count() > 3 ? badge.CompletedLevel * 3 : 0;
                var twoLastAchievement = gameAchievement.Skip(Math.Max(number, gameAchievement.Count - 2)).Take(2).ToList();
                if (twoLastAchievement.Any() && twoLastAchievement.Count() == 2)
                {
                    if (!gameAchievement.Where(x => x.Level == twoLastAchievement.ToArray()[0].Level && x.Id != twoLastAchievement.ToArray()[0].Id).Any() &&
                        !gameAchievement.Where(x => x.Level == twoLastAchievement.ToArray()[1].Level && x.Id != twoLastAchievement.ToArray()[1].Id).Any())
                    {
                        bool areConsecutive = twoLastAchievement.Count() == 2 && twoLastAchievement.ToArray()[0].Level == twoLastAchievement.ToArray()[1].Level - 1 && twoLastAchievement.ToArray()[0].Mark > 0 && twoLastAchievement.ToArray()[1].Mark > 0;
                        if (createAchievementRequest.Level != twoLastAchievement.LastOrDefault().Level && createAchievementRequest.Mark > 0 && areConsecutive && twoLastAchievement.Last().Level + 1 == createAchievementRequest.Level)
                            await GetBadge(account, "Streak killer");
                    }
                }
                #endregion

                #region The Breaker badge
                var highScore = account.Achievements.Where(x => x.AccountId == account.Id && x.Level == achievement.Level && x.GameId == game.Id).OrderByDescending(x => x.Mark).FirstOrDefault();
                if (highScore != null && createAchievementRequest.Mark > highScore.Mark)
                       await GetBadge(account, "The Breaker");
                #endregion

                await _unitOfWork.Repository<Achievement>().CreateAsync(achievement);              

                #region Fast and Furious badge
                if (createAchievementRequest.Duration < 20)
                {
                    if (!gameAchievement.Where(x => x.Level == createAchievementRequest.Level).Any())
                            await GetBadge(account, "Fast and Furious");
                }
                #endregion

                #region Legend badge
                var leaderboard = GetLeaderboard(createAchievementRequest.GameId).Result;
                var top1 = leaderboard.FirstOrDefault();
                var acc = leaderboard.SingleOrDefault(x => x.AccountId == account.Id);
                if ( leaderboard.Count() > 10 && (acc != null && acc.Mark + createAchievementRequest.Mark >= top1?.Mark))
                        await GetBadge(account, "Legend");
                #endregion


                #region Plow Lord Badge
                var list = new List<Achievement>();
                foreach (var achievementOfAccount in account.Achievements)
                {
                    if (list.SingleOrDefault(x => x.GameId == achievementOfAccount.GameId) == null)
                    {
                        if (achievementOfAccount.Level == 10)
                            list.Add(achievementOfAccount);
                    }

                }
                if (account.Achievements.Count(x => x.GameId == createAchievementRequest.GameId && x.Level == 10) == 1)
                        await  GetPlowLordBadge(account, list);
                #endregion

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
        private async Task GetPlowLordBadge(Account account,List<Achievement>list)
        {
            var result = await GetListBadgesCompleted(account);
            if (result.SingleOrDefault(x => x.Challenge.Name.Equals("Plow Lord")) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals("Plow Lord"));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals("Plow Lord"));
                if (badge != null && list.Count() == (badge.CompletedLevel + 1))
                {
                    if (badge.CompletedLevel < challage.CompletedMilestone)
                        badge.CompletedLevel += 1;
                    if (badge.CompletedLevel == challage.CompletedMilestone )
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
                            Status = false,
                            Description = $"You have received {challage.Name} badge.",
                            Title = "ThinkTank"
                        };
                        await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                    }
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }
                if (badge == null)
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
        private async Task GetBadge(Account account, string name)
        {
            var result = await GetListBadgesCompleted(account);
            if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                if (badge != null)
                {
                    if (badge.CompletedLevel < challage.CompletedMilestone)
                        badge.CompletedLevel += 1;
                    if (badge.CompletedLevel == challage.CompletedMilestone )
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
        public async Task<AchievementResponse> GetAchievementById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Achievement Invalid", "");
                }
                var response = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(c=>c.Account).Include(c=>c.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found achievement with id {id}", "");
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get achievement by ID error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AchievementResponse>> GetAchievements(AchievementRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AchievementResponse>(request);
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(x => x.Account).Include(x=>x.Game)
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
        public async Task<PagedResults<LeaderboardResponse>> GetLeaderboard(int id, PagingRequest paging,int? accountId)
        {
            try
            {
                if (id <= 0 || accountId != null && accountId <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == id);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {id} not found", "");

                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(c => c.Account).Include(c => c.Game)
                    .Where(x => x.GameId == id).ToList();
                
                List<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                List<Achievement> achievementsList = new List<Achievement>();
                if (achievements.Count() > 0)
                {
                    //Tính tổng tất cả điểm của các account
                      achievementsList = achievements
                     .GroupBy(achievement => achievement.AccountId)
                     .Select(group => GetSumScoreOfAccount(group.Key, achievements))
                     .Where(rs => rs != null)
                     .ToList();

                    var orderedAccounts = achievementsList.Where(x=>x.Mark >0).OrderByDescending(x => x.Mark);
                    var rank = 1;

                    foreach (var achievement in orderedAccounts)
                    {
                        //Nếu bảng xếp hạng chưa có account 
                        if (responses.Count(a => a.AccountId == achievement.AccountId) == 0)
                        {
                            
                            var leaderboardContestResponse = new LeaderboardResponse
                            {
                                AccountId = achievement.AccountId,
                                Mark = achievement.Mark,
                                Avatar = achievement.Account.Avatar,
                                FullName = achievement.Account.FullName
                            };

                            //List những ai đang đồng hạng 
                            var mark = achievementsList
                                .Where(x => x.Mark == achievement.Mark && x.AccountId != achievement.AccountId)
                                .ToList();

                            if (mark.Any())
                            {
                                //Nếu có lấy người có cùng điểm đầu tiên trong bảng xếp hạng hiện tại
                                var a = responses.SingleOrDefault(a => a.AccountId == mark.First().AccountId);
                                //Gán rank người có cùng điểm đầu tiên trong bảng xếp hạng hiện tại bằng rank của account
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
                    if (accountId != null)
                        responses = responses.Where(x => x.AccountId == accountId).ToList();

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
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(c => c.Account).Include(c => c.Game)
                    .Where(x=>x.GameId==id ).ToList();

                IList<LeaderboardResponse> responses = new List<LeaderboardResponse>();
                List<Achievement> achievementsList = new List<Achievement>();

                if (achievements.Count() > 0)
                {
                     achievementsList = achievements
                    .GroupBy(achievement => achievement.AccountId)
                    .Select(group => GetSumScoreOfAccount(group.Key, achievements))
                    .Where(rs => rs != null)
                     .ToList();

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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get leaderboard of achievement error!!!!!", ex.Message);
            }
        }
        private Achievement GetSumScoreOfAccount(int id, List<Achievement> achievements)
        {
            List<Achievement> responses = new List<Achievement>();
            Account account = null;
            foreach (var achievement in achievements)
            {
                if (responses.Count(a => a.Level == achievement.Level)==0)
                {
                    var highestScore = achievements.Where(x => x.AccountId == id && x.Level == achievement.Level).OrderByDescending(x=>x.Mark).FirstOrDefault();
                    if (highestScore != null)
                    {
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
