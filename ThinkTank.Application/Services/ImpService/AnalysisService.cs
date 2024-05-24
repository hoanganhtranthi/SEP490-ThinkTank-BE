using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.Services.ImpService
{
    public class AnalysisService:IAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IChallengeService _challengeService;
        public AnalysisService(IUnitOfWork unitOfWork, IMapper mapper, IChallengeService challengeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _challengeService = challengeService;
        }
        private  List<GameLevelOfAccountResponse> GetGameLevelByAccountId(int accountId)
        {
                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == accountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");

                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking()
                .Include(x => x.Game)
                    .Where(x => x.AccountId == accountId).ToList();

                var result = new List<GameLevelOfAccountResponse>();
                foreach (var achievement in achievements)
                {
                    GameLevelOfAccountResponse gameLevelOfAccountResponse = new GameLevelOfAccountResponse();
                    var game = result.SingleOrDefault(a => a.GameId == achievement.GameId);
                    if (game == null)
                    {
                        gameLevelOfAccountResponse.GameId = (int)achievement.GameId;
                        gameLevelOfAccountResponse.GameName = achievement.Game.Name;
                        gameLevelOfAccountResponse.Level = achievements.Where(a => a.GameId == achievement.GameId).ToList().OrderByDescending(a => a.Level).Distinct().First().Level;
                        result.Add(gameLevelOfAccountResponse);
                    }
                }
                return result;
        }
        public async Task<dynamic> GetAnalysisOfAccountId(int accountId)
        {
            try
            {
                if (accountId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.AccountInContests)
                                .Include(x => x.Badges)
                                .SingleOrDefault(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "This account is block", "");

                var totalContest = account.AccountInContests.Count();
                var listGameLevel = GetGameLevelByAccountId(accountId);
                var totalLevel = listGameLevel.Sum(x => x.Level);
                var totalBadge = account.Badges.Count();

                var levelOfFlipcard = listGameLevel.FirstOrDefault(x => x.GameName == "Flip Card")?.Level ?? 0;
                var levelOfMusicPassword = listGameLevel.FirstOrDefault(x => x.GameName == "Music Password")?.Level ?? 0;
                var levelOfImagesWalkthrough = listGameLevel.FirstOrDefault(x => x.GameName == "Images Walkthrough")?.Level ?? 0;
                var levelOfFindTheAnonymous = listGameLevel.FirstOrDefault(x => x.GameName == "Find The Anonymous")?.Level ?? 0;


                var request = new ChallengeRequest
                {
                    AccountId = accountId,
                    Status = StatusType.All
                };
                var listArchievements = (await _challengeService.GetChallenges(request)).ToList();

                return new
                {
                    Account = _mapper.Map<AccountResponse>(account),
                    TotalContest = totalContest,
                    TotalLevel = totalLevel,
                    TotalBadge = totalBadge,
                    ListArchievements = listArchievements,
                    PercentOfFlipcard = totalLevel > 0 ? (double)levelOfFlipcard / totalLevel * 100 : 0,
                    PercentOfMusicPassword = totalLevel > 0 ? (double)levelOfMusicPassword / totalLevel * 100 : 0,
                    PercentOfImagesWalkthrough = totalLevel > 0 ? (double)levelOfImagesWalkthrough / totalLevel * 100 : 0,
                    PercentOfFindTheAnonymous = totalLevel > 0 ? (double)levelOfFindTheAnonymous / totalLevel * 100 : 0
                };
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get analysis of account by account Id error!!!!!", ex.Message);
            }
        }
        public async Task<dynamic> GetAnalysisOfAccountIdAndGameId(AnalysisRequest request)
        {
            try
            {
                if (request.AccountId <= 0 || request.GameId <=0 || request.FilterMonth !=null && request.FilterMonth <=0 || request.FilterYear != null && request.FilterYear<=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.Achievements.Where(x => x.GameId == request.GameId))
                                .SingleOrDefault(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} not found ", "");
                if (account.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                var result = account.Achievements
                    .Where(achievement => achievement.Duration > 0)
                    .Select(achievement => new
                    {
                        EndTime = achievement.CompletedTime.Date,
                        Value = achievement.Duration > 0 ? (double)(achievement.PieceOfInformation / achievement.Duration):0
                    })
                    .ToList();

                if(request.FilterMonth != null && request.FilterYear==null || request.FilterMonth==null && request.FilterYear !=null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Please enter year and month ", "");

                if (request.FilterMonth != null && request.FilterYear != null)
                {                        
                    result = result.Where(x => x.EndTime.Month == request.FilterMonth && x.EndTime.Year == request.FilterYear).ToList();
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account by Account Id and Game Id error!!!!!", ex.Message);
            }
        }
        public async Task<dynamic> GetAnalysisOfMemoryTypeByAccountId(int accountId)
        {
            try
            {
                if (accountId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.Achievements)
                                .SingleOrDefault(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");
                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                var listGameLevel = GetGameLevelByAccountId(accountId);

                //Get level cao nhất của từng game
                var levelOfFlipcard = listGameLevel.FirstOrDefault(x => x.GameName == "Flip Card")?.Level ?? 0;
                var levelOfMusicPassword = listGameLevel.FirstOrDefault(x => x.GameName == "Music Password")?.Level ?? 0;
                var levelOfImagesWalkthrough = listGameLevel.FirstOrDefault(x => x.GameName == "Images Walkthrough")?.Level ?? 0;
                var levelOfFindTheAnonymous = listGameLevel.FirstOrDefault(x => x.GameName == "Find The Anonymous")?.Level ?? 0;

                //Tính tổng level của các game tương ứng với mỗi loại trí nhớ
                var percentOfImagesMemory = ((double)(levelOfFlipcard + levelOfImagesWalkthrough + levelOfFindTheAnonymous));
                var percentOfAudioMemory=levelOfMusicPassword;
                var percentOfSequentialMemory = ((double)(levelOfMusicPassword + levelOfImagesWalkthrough));

                var totalPercent = percentOfAudioMemory + percentOfImagesMemory + percentOfSequentialMemory;

                return new
                {
                    PercentOfImagesMemory= percentOfImagesMemory==0?0 :percentOfImagesMemory/totalPercent*100,
                    PercentOfAudioMemory=percentOfAudioMemory==0? 0:percentOfAudioMemory/totalPercent*100,  
                    PercentOfSequentialMemory=percentOfSequentialMemory==0?0: percentOfSequentialMemory /totalPercent * 100
                };
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account's Memory Type error!!!!!", ex.Message);
            }
        }

        public async Task<AnalysisAverageScoreResponse> GetAverageScoreAnalysis(int gameId, int userId)
        {
            try
            {
                if (gameId<=0 || userId <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == gameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Game Id {gameId} is not found", "");

                var acc = _unitOfWork.Repository<Account>().Find(x => x.Id == userId);
                if (acc == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {userId} is not found", "");
                if(acc.Status==false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {gameId} is block", "");

                //Get achievements theo game
                var achievements = _unitOfWork.Repository<Achievement>()
                    .GetAll()
                    .AsNoTracking()
                    .Where(x => x.GameId == gameId)
                    .ToList();

                //Get level hiện tại cua account
                var currentLevel = achievements.Where(x => x.AccountId == userId).OrderByDescending(a => a.Level).Distinct().FirstOrDefault();
                
                var analysisAverageScore = new AnalysisAverageScoreResponse
                {
                    UserId = userId,
                    CurrentLevel=currentLevel != null ? currentLevel.Level :0 ,
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
                var maxLevel = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.GameId == gameId).ToList().OrderByDescending(a => a.Level).Distinct().FirstOrDefault();
                
                // Tính mảnh thông tin/thời gian theo từng level
                var averageScoresByLevel = achievementsOfLevels
                    .GroupBy(a => a.Level)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(a => a.AccountId == userId).Select(a => a.Duration > 0 ? (double)(a.PieceOfInformation / a.Duration) : 0).FirstOrDefault()
                    );

                // Tính toán trung bình của từng nhóm cấp độ chơi
                var groupLevels = new List<string> { "Level 1", "Level 2 - Level 5", "Level 6 - Level 10", "Level 11 - Level 20", "Level 21 - Level 30", "Level 31 - Level 40" };

                var maxLevelOfGame = maxLevel != null ? maxLevel.Level : 0;
                groupLevels.Add(maxLevelOfGame > 41 ? $"Level 41 - Level {maxLevelOfGame}" : "Level above 41");

                foreach (var groupLevel in groupLevels)
                {
                    var range = GetLevelRange(groupLevel, gameId);

                    var averageOfPlayer = range[1] >= range[0] ? Enumerable.Range(range[0], range[1] - range[0] + 1)
                        .Select(level => averageScoresByLevel.ContainsKey(level) ? averageScoresByLevel[level] : 0)
                        .Average():0;

                    var averageOfGroup = range[1] >= range[0] ? achievementsOfLevels
                        .Where(a => range[0] <= a.Level && a.Level <= range[1])
                        .GroupBy(a => a.AccountId)
                        .Select(group => group.Average(a => a.Duration > 0 ? (double)(a.PieceOfInformation / a.Duration) : 0))
                        .DefaultIfEmpty(0)
                        .Average():0;


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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account's Average Score error!!!!!", ex.Message);
            }

        }
        private List<int> GetLevelRange(string groupLevel, int gameId)
        {
             var maxLevelOfGame=_unitOfWork.Repository<Achievement>()
                .GetAll().AsNoTracking().Where(x => x.GameId == gameId).ToList().OrderByDescending(a => a.Level).Distinct().FirstOrDefault() ;

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
                    return new List<int> { 41, maxLevelOfGame!=null?maxLevelOfGame.Level:0 };

            }
        }
    }
}
