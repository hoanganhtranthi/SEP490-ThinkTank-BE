﻿using AutoMapper;
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
                var topic = _unitOfWork.Repository<Topic>().GetAll().Include(x=>x.Game).SingleOrDefault(x => x.Id == createAchievementRequest.TopicId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic id {createAchievementRequest.TopicId} is not found !!!", "");

                var account = _unitOfWork.Repository<Account>().GetAll().Include(x=>x.Achievements).SingleOrDefault(x => x.Id == createAchievementRequest.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account id {createAchievementRequest.AccountId} is not found !!!", "");

                var levels = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.AccountId == createAchievementRequest.AccountId && createAchievementRequest.TopicId == x.TopicId).OrderBy(x => x.Level).ToList();
                var level = 0;
                if (levels.Count() > 0)
                    level = levels.LastOrDefault().Level;
                else level = 0;
                if (createAchievementRequest.Level > level + 1 || createAchievementRequest.Level <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Level", "");

                achievement.CompletedTime = DateTime.Now;
                await _unitOfWork.Repository<Achievement>().CreateAsync(achievement);
                var leaderboard = GetLeaderboard((int)topic.GameId).Result;
                var top1 = leaderboard.FirstOrDefault();
                var topAccountId = top1?.AccountId;

                if (createAchievementRequest.Duration < 20)
                    GetBadge(account, "Fast and Furious");

                var acc = leaderboard.SingleOrDefault(x => x.AccountId == account.Id);
                if ((account.Achievements.Any(x => x.TopicId == createAchievementRequest.TopicId && x.Level == createAchievementRequest.Level) &&
                    (acc != null && acc.Mark + createAchievementRequest.Mark >= top1?.Mark)))
                {
                    GetBadge(account, "Legend");
                }
                var list = new List<Achievement>();
                foreach (var result in account.Achievements)
                {
                    var t = _unitOfWork.Repository<Topic>().GetAll().Include(x => x.Game).SingleOrDefault(x => x.Id == result.TopicId);
                    if (list.SingleOrDefault(x => x.Topic.GameId == t.GameId) == null)
                    {
                        if (result.Level == 10)
                            list.Add(result);
                    }
                }
                var highScore = account.Achievements.Where(x => x.AccountId == account.Id && x.Level == achievement.Level && x.TopicId==topic.Id).OrderByDescending(x => x.Mark).FirstOrDefault();
                if (createAchievementRequest.Mark > highScore.Mark)
                    GetBadge(account, "The Breaker");
                if (account.Achievements.Count(x => x.TopicId == createAchievementRequest.TopicId && x.Level == 10) == 1)
                {
                    GetPlowLordBadge(account, list);
                }

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AchievementResponse>(achievement);
                rs.Username = account.UserName;
                rs.GameName = topic.Game.Name;
                rs.TopicName = topic.Name;
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
            if (badge != null && list.Count() == (badge.CompletedLevel + 1))
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
        public async Task<AchievementResponse> GetAchievementById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Achievement Invalid", "");
                }
                var response = _unitOfWork.Repository<Achievement>().GetAll().Include(c=>c.Account).Include(c=>c.Topic.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found achievement with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AchievementResponse>(response);
                rs.Username=response.Account.UserName;
                rs.GameName=response.Topic.Game.Name;
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
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Account).Include(x => x.Topic.Game).Include(x=>x.Topic)
                    .Select(x => new AchievementResponse
                    {
                        Id = x.Id,
                        GameName = x.Topic.Game.Name,
                        AccountId=x.AccountId,
                        CompletedTime=x.CompletedTime,
                        Duration=x.Duration,
                       TopicId=x.TopicId,
                       PieceOfInformation=x.PieceOfInformation,
                       TopicName=x.Topic.Name,
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

        public async Task<List<LeaderboardResponse>> GetLeaderboard(int id)
        {
            try
            {
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().Include(c => c.Account).Include(c => c.Topic.Game)
                    .Where(x=>x.Topic.GameId==id ).ToList();

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
        public Achievement GetSumScoreOfAccount(int id, List<Achievement> achievements)
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
