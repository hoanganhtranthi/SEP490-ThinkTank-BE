
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
    public class ChallengeService : IChallengeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ChallengeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<ChallengeResponse>> GetChallenges(ChallengeRequest request)
        {
            try
            {
                var challenges = _unitOfWork.Repository<Challenge>().GetAll().AsNoTracking().Include(x=>x.Badges)
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
                if (request.Status != StatusType.All)
                {
                    bool? status = null;
                    if (request.Status.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.Status.ToString().ToLower());
                    }
                    challenges = challenges.Where(x => x.Status.Equals(status)).ToList();
                }
                return challenges;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get challenge list error!!!!!", ex.Message);
            }
        }
        public async Task<List<ChallengeResponse>> GetCoinReward(int accountId, int? challengeId)
        {
            try
            {
                if (accountId <= 0 || challengeId != null && challengeId <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var acc = _unitOfWork.Repository<Account>().GetAll().Include(x => x.Badges).SingleOrDefault(x => x.Id == accountId);

                if (acc == null)
                    throw new  CrudException(HttpStatusCode.NotFound, $"Account Id {accountId} is not found ","");

                if (acc.Status == false) 
                    throw new CrudException(HttpStatusCode.BadRequest, $"Your account {acc.Id} is block", "");

                if (challengeId != null)
                {
                    var challenge = _unitOfWork.Repository<Challenge>().Find(x => x.Id == challengeId);

                    if (challenge == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Challegne Id {challengeId} is not found ", "");

                    var badge = acc.Badges.SingleOrDefault(x => x.ChallengeId == challengeId);

                    if (badge == null)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} has not yet taken the challenge {challenge.Name}", "");

                    if ( badge.CompletedLevel != challenge.CompletedMilestone)
                        throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {accountId} haven't completed the correct milestones for this challenge ", "");
                    
                    badge.Status = true;
                   
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                    acc.Coin += 20;
                    
                }

                if(acc.Badges.Where(x=>x.CompletedLevel==_unitOfWork.Repository<Challenge>().Find(a=>a.Id==x.ChallengeId).CompletedMilestone).Count()==10)
                    acc.Coin += 1000;

                await _unitOfWork.Repository<Account>().Update(acc, accountId);
                await _unitOfWork.CommitAsync();

                ChallengeRequest request = new ChallengeRequest();
                request.AccountId = accountId;
                request.Status = StatusType.All;

                return GetChallenges(request).Result;

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
