
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfMemoryTypeByAccountId
{
    public class GetAnalysisOfMemoryTypeByAccountIdQueryHandler : IQueryHandler<GetAnalysisOfMemoryTypeByAccountIdQuery, AnalysisOfMemoryTypeResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetAnalysisOfMemoryTypeByAccountIdQueryHandler(IUnitOfWork unitOfWork,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<AnalysisOfMemoryTypeResponse> Handle(GetAnalysisOfMemoryTypeByAccountIdQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                .Include(x => x.Achievements)
                                .SingleOrDefault(x => x.Id == request.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} not found ", "");
                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                var listGameLevel = GetGameLevelByAccountId(request.AccountId);

                //Get level cao nhất của từng game
                var levelOfFlipcard = listGameLevel.FirstOrDefault(x => x.GameName == "Flip Card")?.Level ?? 0;
                var levelOfMusicPassword = listGameLevel.FirstOrDefault(x => x.GameName == "Music Password")?.Level ?? 0;
                var levelOfImagesWalkthrough = listGameLevel.FirstOrDefault(x => x.GameName == "Images Walkthrough")?.Level ?? 0;
                var levelOfFindTheAnonymous = listGameLevel.FirstOrDefault(x => x.GameName == "Find The Anonymous")?.Level ?? 0;

                //Tính tổng level của các game tương ứng với mỗi loại trí nhớ
                var percentOfImagesMemory = ((double)(levelOfFlipcard + levelOfImagesWalkthrough + levelOfFindTheAnonymous));
                var percentOfAudioMemory = levelOfMusicPassword;
                var percentOfSequentialMemory = ((double)(levelOfMusicPassword + levelOfImagesWalkthrough));

                var totalPercent = percentOfAudioMemory + percentOfImagesMemory + percentOfSequentialMemory;

                return new AnalysisOfMemoryTypeResponse
                {
                    PercentOfImagesMemory = percentOfImagesMemory == 0 ? 0 : percentOfImagesMemory / totalPercent * 100,
                    PercentOfAudioMemory = percentOfAudioMemory == 0 ? 0 : percentOfAudioMemory / totalPercent * 100,
                    PercentOfSequentialMemory = percentOfSequentialMemory == 0 ? 0 : percentOfSequentialMemory / totalPercent * 100
                };
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Analysis of Account's Memory Type error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account's Memory Type error!!!!!", ex.Message);
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
