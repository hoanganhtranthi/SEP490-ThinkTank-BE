
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Queries.GetGameLevelByAccountId
{
    public class GetGameLevelByAccountIdQueryHandler : IQueryHandler<GetGameLevelByAccountIdQuery, List<GameLevelOfAccountResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetGameLevelByAccountIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GameLevelOfAccountResponse>> Handle(GetGameLevelByAccountIdQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.Id);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.Id} not found ", "");

                if (account.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                var achievements = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(x => x.Game)
                    .Where(x => x.AccountId == request.Id).ToList();

                var result = new List<GameLevelOfAccountResponse>();
                foreach (var achievement in achievements)
                {
                    GameLevelOfAccountResponse gameLevelOfAccountResponse = new GameLevelOfAccountResponse();

                    var game = result.SingleOrDefault(a => a.GameId == achievement.GameId);
                    if (game == null)
                    {
                        gameLevelOfAccountResponse.GameId = (int)achievement.GameId;
                        gameLevelOfAccountResponse.GameName = achievement.Game.Name;
                        gameLevelOfAccountResponse.Level = achievements.Where(a => a.GameId == achievement.GameId).OrderByDescending(a => a.Level).Distinct().First().Level;
                        result.Add(gameLevelOfAccountResponse);
                    }
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get game level of account error!!!!!", ex.Message);
            }
        }
    }
}
