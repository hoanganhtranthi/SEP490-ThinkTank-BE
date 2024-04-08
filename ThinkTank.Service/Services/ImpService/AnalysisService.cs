using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
using ThinkTank.Service.Services.IService;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.Services.ImpService
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
                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(x => x.Account).Include(x => x.Game)
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
                        gameLevelOfAccountResponse.Level = achievements.LastOrDefault(a => a.GameId == achievement.GameId).Level;
                        result.Add(gameLevelOfAccountResponse);
                    }
                }
                return result;
        }
        public async Task<dynamic> GetAnalysisOfAccountId(int accountId)
        {
            try
            {
                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.AccountInContests)
                                .Include(x => x.Badges)
                                .SingleOrDefault(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");

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
                    Status = Helpers.Enum.StatusType.All
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account error!!!!!", ex.Message);
            }
        }
        public async Task<dynamic> GetAnalysisOfAccountIdAndGameId(AnalysisRequest request)
        {
            try
            {
                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.Achievements.Where(x => x.GameId == request.GameId))
                                .SingleOrDefault(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} not found ", "");

                var result = account.Achievements
                    .Where(achievement => achievement.Duration > 0)
                    .Select(achievement => new
                    {
                        EndTime = achievement.CompletedTime.Date,
                        Value = (double)(achievement.PieceOfInformation / achievement.Duration)
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account error!!!!!", ex.Message);
            }
        }
        public async Task<dynamic> GetAnalysisOfMemoryTypeByAccountId(int accountId)
        {
            try
            {
                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.Achievements)
                                .SingleOrDefault(x => x.Id == accountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} not found ", "");
                var listGameLevel = GetGameLevelByAccountId(accountId);

                var levelOfFlipcard = listGameLevel.FirstOrDefault(x => x.GameName == "Flip Card")?.Level ?? 0;
                var levelOfMusicPassword = listGameLevel.FirstOrDefault(x => x.GameName == "Music Password")?.Level ?? 0;
                var levelOfImagesWalkthrough = listGameLevel.FirstOrDefault(x => x.GameName == "Images Walkthrough")?.Level ?? 0;
                var levelOfFindTheAnonymous = listGameLevel.FirstOrDefault(x => x.GameName == "Find The Anonymous")?.Level ?? 0;
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
    }
}
