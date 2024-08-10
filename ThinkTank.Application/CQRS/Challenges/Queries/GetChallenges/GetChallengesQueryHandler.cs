

using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.Challenges.Queries.GetChallenges
{
    public class GetChallengesQueryHandler : IQueryHandler<GetChallengesQuery, List<ChallengeResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetChallengesQueryHandler(IUnitOfWork unitOfWork,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<List<ChallengeResponse>> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var challenges = _unitOfWork.Repository<Challenge>().GetAll().AsNoTracking().Include(x => x.Badges)
                                           .Select(x => new ChallengeResponse
                                           {
                                               Id = x.Id,
                                               Avatar = x.Avatar,
                                               CompletedMilestone = x.CompletedMilestone,
                                               Description = x.Description,
                                               MissionsImg = x.MissionsImg,
                                               Name = x.Name,
                                               Unit = x.Unit,
                                               CompletedLevel = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.ChallengeRequest.AccountId).CompletedLevel,
                                               CompletedDate = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.ChallengeRequest.AccountId).CompletedDate,
                                               Status = x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.ChallengeRequest.AccountId).Status
                                           })
                                           .ToList();
                if (request.ChallengeRequest.Status != StatusType.All)
                {
                    bool? status = null;
                    if (request.ChallengeRequest.Status.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.ChallengeRequest.Status.ToString().ToLower());
                    }
                    challenges = challenges.Where(x => x.Status.Equals(status)).ToList();
                }
                return challenges;
            }
            catch (CrudException ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get challenge list error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get challenge list error!!!!!", ex.Message);
            }
        }
    }
}
