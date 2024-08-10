
using AutoMapper;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Analysis.Queries.GetAverageScoreAnalysis
{
    public class GetAverageScoreAnalysisQueryHandler : IQueryHandler<GetAverageScoreAnalysisQuery, AnalysisAverageScoreResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetAverageScoreAnalysisQueryHandler(IUnitOfWork unitOfWork,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }
        public async Task<AnalysisAverageScoreResponse> Handle(GetAverageScoreAnalysisQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {request.GameId} is not found", "");

                var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == request.AccountId);
                if (acc == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} is not found", "");
                if (acc.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} is block", "");

                //Get achievements theo game
                var achievements = _unitOfWork.Repository<Achievement>()
                    .GetAll()
                .AsNoTracking()
                .Where(x => x.GameId == request.GameId)
                    .ToList();

                //Get level hiện tại cua account
                var currentLevel = achievements.Where(x => x.AccountId == request.AccountId).OrderByDescending(a => a.Level).Distinct().FirstOrDefault();

                var analysisAverageScore = new AnalysisAverageScoreResponse
                {
                    UserId = request.AccountId,
                    CurrentLevel = currentLevel != null ? currentLevel.Level : 0,
                    AnalysisAverageScoreByGroupLevelResponses = new List<AnalysisAverageScoreByGroupLevelResponse>()
                };

                //Get list achievemts lần đầu tiên theo level group by theo account Id
                var achievementsOfLevels = achievements
                    .GroupBy(a => new { a.Level, a.AccountId })
                    .Select(g => new Achievement
                    {
                        Level = g.Key.Level,
                        AccountId = g.Key.AccountId,
                        Duration = g.First().Duration,
                        PieceOfInformation = g.First().PieceOfInformation,
                        Mark = g.First().Mark,
                        Id = g.First().Id
                    })
                    .ToList();

                // Get level cao nhất của game 
                var maxLevel = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.GameId == request.GameId).ToList().OrderByDescending(a => a.Level).Distinct().FirstOrDefault();

                // Tính mảnh thông tin/thời gian theo từng level
                var averageScoresByLevel = achievementsOfLevels
                .GroupBy(a => a.Level)
                .ToDictionary(
                        g => g.Key,
                        g => g.Where(a => a.AccountId == request.AccountId).Select(a => a.Duration > 0 ? (double)(a.PieceOfInformation / a.Duration) : 0).FirstOrDefault()
                    );

                // Tính toán trung bình của từng nhóm cấp độ chơi
                var groupLevels = new List<string> { "Level 1", "Level 2 - Level 5", "Level 6 - Level 10", "Level 11 - Level 20", "Level 21 - Level 30", "Level 31 - Level 40" };

                var maxLevelOfGame = maxLevel != null ? maxLevel.Level : 0;
                groupLevels.Add(maxLevelOfGame > 41 ? $"Level 41 - Level {maxLevelOfGame}" : "Level above 41");

                foreach (var groupLevel in groupLevels)
                {
                    var range = GetLevelRange(groupLevel, request.GameId);

                    var averageOfPlayer = range[1] >= range[0] ? Enumerable.Range(range[0], range[1] - range[0] + 1)
                        .Select(level => averageScoresByLevel.ContainsKey(level) ? averageScoresByLevel[level] : 0)
                        .Average() : 0;

                    var averageOfGroup = range[1] >= range[0] ? achievementsOfLevels
                        .Where(a => range[0] <= a.Level && a.Level <= range[1])
                        .GroupBy(a => a.AccountId)
                        .Select(group => group.Average(a => a.Duration > 0 ? (double)(a.PieceOfInformation / a.Duration) : 0))
                        .DefaultIfEmpty(0)
                        .Average() : 0;


                    analysisAverageScore.AnalysisAverageScoreByGroupLevelResponses.Add(new AnalysisAverageScoreByGroupLevelResponse
                    {
                        GroupLevel = groupLevel,
                        AverageOfPlayer = averageOfPlayer,
                        AverageOfGroup = averageOfGroup
                    });
                }

                return analysisAverageScore;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Analysis of Account's Average Score error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account's Average Score error!!!!!", ex.Message);
            }

        }
        private List<int> GetLevelRange(string groupLevel, int gameId)
        {
            var maxLevelOfGame = _unitOfWork.Repository<Achievement>()
               .GetAll().AsNoTracking().Where(x => x.GameId == gameId).ToList().OrderByDescending(a => a.Level).Distinct().FirstOrDefault();

            switch (groupLevel)
            {
                case "Level 1":
                    return new List<int> { 1, 1 };
                case "Level 2 - Level 5":
                    return new List<int> { 2, 5 };
                case "Level 6 - Level 10":
                    return new List<int> { 6, 10 };
                case "Level 11 - Level 20":
                    return new List<int> { 11, 20 };
                case "Level 21 - Level 30":
                    return new List<int> { 21, 30 };
                case "Level 31 - Level 40":
                    return new List<int> { 31, 40 };
                default:
                    return new List<int> { 41, maxLevelOfGame != null ? maxLevelOfGame.Level : 0 };

            }
        }
    }
}
