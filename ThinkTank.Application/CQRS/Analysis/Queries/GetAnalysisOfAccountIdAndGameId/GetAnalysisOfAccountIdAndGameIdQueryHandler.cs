

using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountIdAndGameId
{
    public class GetAnalysisOfAccountIdAndGameIdQueryHandler : IQueryHandler<GetAnalysisOfAccountIdAndGameIdQuery, List<RatioMemorizedDailyResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetAnalysisOfAccountIdAndGameIdQueryHandler(IUnitOfWork unitOfWork,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<List<RatioMemorizedDailyResponse>> Handle(GetAnalysisOfAccountIdAndGameIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.AnalysisRequest.AccountId <= 0 || request.AnalysisRequest.GameId <= 0 || request.AnalysisRequest.FilterMonth != null && request.AnalysisRequest.FilterMonth <= 0 || request.AnalysisRequest.FilterYear != null && request.AnalysisRequest.FilterYear <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var account = _unitOfWork.Repository<Account>()
                                .GetAll().AsNoTracking()
                                .Include(x => x.Achievements.Where(x => x.GameId == request.AnalysisRequest.GameId))
                                .SingleOrDefault(x => x.Id == request.AnalysisRequest.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AnalysisRequest.AccountId} not found ", "");
                if (account.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                var result = account.Achievements
                    .Where(achievement => achievement.Duration > 0)
                    .Select(achievement => new RatioMemorizedDailyResponse
                    {
                        EndTime = achievement.CompletedTime.Date,
                        Value = achievement.Duration > 0 ? (double)(achievement.PieceOfInformation / achievement.Duration) : 0
                    })
                    .ToList();

                if (request.AnalysisRequest.FilterMonth != null && request.AnalysisRequest.FilterYear == null || request.AnalysisRequest.FilterMonth == null && request.AnalysisRequest.FilterYear != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Please enter year and month ", "");

                if (request.AnalysisRequest.FilterMonth != null && request.AnalysisRequest.FilterYear != null)
                {
                    result = result.Where(x => x.EndTime.Month == request.AnalysisRequest.FilterMonth && x.EndTime.Year == request.AnalysisRequest.FilterYear).ToList();
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Analysis of Account by Account Id and Game Id error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Analysis of Account by Account Id and Game Id error!!!!!", ex.Message);
            }
        }
    }
}
