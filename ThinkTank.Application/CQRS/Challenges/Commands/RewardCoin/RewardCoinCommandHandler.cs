

using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Challenges.Commands.RewardCoin
{
    public class RewardCoinCommandHandler : ICommandHandler<RewardCoinCommand, List<ChallengeResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public RewardCoinCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ChallengeResponse>> Handle(RewardCoinCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.AccountId <= 0 || request.ChallengeId != null && request.ChallengeId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var acc = _unitOfWork.Repository<Account>().GetAll().Include(x => x.Badges).SingleOrDefault(x => x.Id == request.AccountId);

                if (acc == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId} is not found ", "");

                if (acc.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Your account {acc.Id} is block", "");

                if (request.ChallengeId != null)
                {
                    var challenge = _unitOfWork.Repository<Challenge>().Find(x => x.Id == request.ChallengeId);

                    if (challenge == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Challegne Id {request.ChallengeId} is not found ", "");

                    var badge = acc.Badges.SingleOrDefault(x => x.ChallengeId == request.ChallengeId);

                    if (badge == null)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} has not yet taken the challenge {challenge.Name}", "");

                    if (badge.CompletedLevel != challenge.CompletedMilestone)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId} haven't completed the correct milestones for this challenge ", "");

                    badge.Status = true;

                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                    acc.Coin += 20;
                }

                if (acc.Badges.Where(x => x.CompletedLevel == _unitOfWork.Repository<Challenge>().Find(a => a.Id == x.ChallengeId).CompletedMilestone).Count() == 10)
                    acc.Coin += 1000;

                await _unitOfWork.Repository<Account>().Update(acc,request.AccountId);
                await _unitOfWork.CommitAsync();

                return _unitOfWork.Repository<Challenge>().GetAll().AsNoTracking().Include(x=>x.Badges)
                                           .Select(x=>new ChallengeResponse
                                           {
                                               Id=x.Id,
                                               Avatar=x.Avatar,
                                               CompletedMilestone=x.CompletedMilestone,
                                               Description=x.Description,
                                               MissionsImg=x.MissionsImg,
                                               Name = x.Name,
                                               Unit=x.Unit,
                                               CompletedLevel= x.Badges.SingleOrDefault(a=>a.ChallengeId==x.Id && a.AccountId==request.AccountId).CompletedLevel,
                                               CompletedDate= x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).CompletedDate,
                                               Status= x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).Status
                                           })
                                           .ToList();

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, " Get reward error!!!!!", ex.Message);
            }
        }
    }
}
