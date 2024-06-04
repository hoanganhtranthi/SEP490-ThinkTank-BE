
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountId
{
    public class GetAnalysisOfAccountIdQueryHandler : IQueryHandler<GetAnalysisOfAccountIdQuery, AdminDashboardResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAnalysisOfAccountIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AdminDashboardResponse> Handle(GetAnalysisOfAccountIdQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                .Include(x => x.AccountInContests)
                                .Include(x => x.Badges)
                                .SingleOrDefault(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} not found ", "");

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "This account is block", "");

                var totalContest = account.AccountInContests.Count();
                var listGameLevel = GetGameLevelByAccountId(request.AccountId);
                var totalLevel = listGameLevel.Sum(x => x.Level);
                var totalBadge = account.Badges.Count();

                var levelOfFlipcard = listGameLevel.FirstOrDefault(x => x.GameName == "Flip Card")?.Level ?? 0;
                var levelOfMusicPassword = listGameLevel.FirstOrDefault(x => x.GameName == "Music Password")?.Level ?? 0;
                var levelOfImagesWalkthrough = listGameLevel.FirstOrDefault(x => x.GameName == "Images Walkthrough")?.Level ?? 0;
                var levelOfFindTheAnonymous = listGameLevel.FirstOrDefault(x => x.GameName == "Find The Anonymous")?.Level ?? 0;


                var listArchievements = _unitOfWork.Repository<Challenge>().GetAll().AsNoTracking().Include(x => x.Badges)
                                           .Select(x => new ChallengeResponse
                                           {
                                               Id = x.Id,
                                               Avatar = x.Avatar,
                                               CompletedMilestone = x.CompletedMilestone,
                                               Description = x.Description,
                                               MissionsImg = x.MissionsImg,
                                               Name = x.Name,
                                               Unit = x.Unit,
                                               CompletedLevel = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).CompletedLevel,
                                               CompletedDate = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).CompletedDate,
                                               Status = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).Status
                                           })
                                           .ToList();

                return new AdminDashboardResponse
                {
                    AccountResponse = _mapper.Map<AccountResponse>(account),
                    TotalContest = totalContest,
                    TotalLevel = totalLevel,
                    TotalBadge = totalBadge,
                    ChallengeResponses = listArchievements,
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
        private List<GameLevelOfAccountResponse> GetGameLevelByAccountId(int accountId)
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
    }
}
