

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Achieviements.Commands.CreateAchievement
{
    public class CreateAchievementCommandHandler : ICommandHandler<CreateAchievementCommand, AchievementResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        public IBadgesService _badgesService;
        public CreateAchievementCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBadgesService badgesService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _badgesService = badgesService;
        }
        public async Task<AchievementResponse> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAchievementRequest.AccountId <= 0 || request.CreateAchievementRequest.Duration < 0 || request.CreateAchievementRequest.Level <= 0 || request.CreateAchievementRequest.Mark < 0
                || request.CreateAchievementRequest.PieceOfInformation <= 0 || request.CreateAchievementRequest.GameId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");
                var achievement = _mapper.Map<CreateAchievementRequest, Achievement>(request.CreateAchievementRequest);

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.CreateAchievementRequest.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This game id {request.CreateAchievementRequest.GameId} is not found !!!", "");

                var account = _unitOfWork.Repository<Account>().GetAll().Include(x => x.Achievements).SingleOrDefault(x => x.Id == request.CreateAchievementRequest.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account id {request.CreateAchievementRequest.AccountId} is not found !!!", "");

                if (account.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {account.Id} not available!!!!!", "");

                //Check level có đúng hay không (không lớn hơn level hiện tại +1)
                var levels = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.AccountId == request.CreateAchievementRequest.AccountId && request.CreateAchievementRequest.GameId == x.GameId).OrderBy(x => x.Level).ToList();
                var level = 0;
                if (levels.Count() > 0)
                    level = levels.LastOrDefault().Level;
                else level = 0;
                if (request.CreateAchievementRequest.Level > level + 1 || request.CreateAchievementRequest.Level <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Invalid Level", "");

                achievement.Account = account;
                achievement.Game = game;
                achievement.CompletedTime = date;

                #region Streak killer badge
                var gameAchievement = account.Achievements.Where(x => x.GameId == game.Id).ToList();
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.Challenge.Name == "Streak killer" && x.AccountId == account.Id);
                var number = badge != null && gameAchievement.Count() > 3 ? badge.CompletedLevel * 3 : 0;
                var twoLastAchievement = gameAchievement.Skip(Math.Max(number, gameAchievement.Count - 2)).Take(2).ToList();
                if (twoLastAchievement.Any() && twoLastAchievement.Count() == 2)
                {
                    if (!gameAchievement.Where(x => x.Level == twoLastAchievement.ToArray()[0].Level && x.Id != twoLastAchievement.ToArray()[0].Id).Any() &&
                        !gameAchievement.Where(x => x.Level == twoLastAchievement.ToArray()[1].Level && x.Id != twoLastAchievement.ToArray()[1].Id).Any())
                    {
                        bool areConsecutive = twoLastAchievement.Count() == 2 && twoLastAchievement.ToArray()[0].Level == twoLastAchievement.ToArray()[1].Level - 1 && twoLastAchievement.ToArray()[0].Mark > 0 && twoLastAchievement.ToArray()[1].Mark > 0;
                        if (request.CreateAchievementRequest.Level != twoLastAchievement.LastOrDefault().Level && request.CreateAchievementRequest.Mark > 0 && areConsecutive && twoLastAchievement.Last().Level + 1 == request.CreateAchievementRequest.Level)
                            await _badgesService.GetBadge(account, "Streak killer");
                    }
                }
                #endregion

                #region The Breaker badge
                var highScore = account.Achievements.Where(x => x.AccountId == account.Id && x.Level == achievement.Level && x.GameId == game.Id).OrderByDescending(x => x.Mark).FirstOrDefault();
                if (highScore != null && request.CreateAchievementRequest.Mark > highScore.Mark)
                    await _badgesService.GetBadge(account, "The Breaker");
                #endregion

                await _unitOfWork.Repository<Achievement>().CreateAsync(achievement);

                #region Fast and Furious badge
                if (request.CreateAchievementRequest.Duration < 20)
                {
                    if (!gameAchievement.Where(x => x.Level == request.CreateAchievementRequest.Level).Any())
                        await _badgesService.GetBadge(account, "Fast and Furious");
                }
                #endregion

                #region Legend badge
                var leaderboard = GetLeaderboard(request.CreateAchievementRequest.GameId).Result;
                var top1 = leaderboard.FirstOrDefault();
                var acc = leaderboard.SingleOrDefault(x => x.AccountId == account.Id);
                if (leaderboard.Count() > 10 && acc != null && acc.Mark + request.CreateAchievementRequest.Mark >= top1?.Mark)
                    await _badgesService.GetBadge(account, "Legend");
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
                if (account.Achievements.Count(x => x.GameId == request.CreateAchievementRequest.GameId && x.Level == 10) == 1)
                    await _badgesService.GetPlowLordBadge(account, list);
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

        private async Task<List<LeaderboardResponse>> GetLeaderboard(int id)
        {
            var game = _unitOfWork.Repository<Game>().Find(x => x.Id == id);
            if (game == null)
                throw new CrudException(HttpStatusCode.NotFound, $"Game Id {id} not found", "");
            var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(c => c.Account).Include(c => c.Game)
                .Where(x => x.GameId == id).ToList();

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
            return responses.ToList();
        }
        private Achievement GetSumScoreOfAccount(int id, List<Achievement> achievements)
        {
            List<Achievement> responses = new List<Achievement>();
            Account account = null;
            foreach (var achievement in achievements)
            {
                if (responses.Count(a => a.Level == achievement.Level) == 0)
                {
                    var highestScore = achievements.Where(x => x.AccountId == id && x.Level == achievement.Level).OrderByDescending(x => x.Mark).FirstOrDefault();
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

