

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Commands.JoinContest
{
    public class JoinContestCommandHandler : ICommandHandler<JoinContestCommand, AccountInContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public JoinContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AccountInContestResponse> Handle(JoinContestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAccountInContestRequest.ContestId <= 0 || request.CreateAccountInContestRequest.AccountId <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var acc = _unitOfWork.Repository<Account>().Find(a => a.Id == request.CreateAccountInContestRequest.AccountId);
                if (acc == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateAccountInContestRequest.AccountId} Not Found!!!!!", "");
                }
                if (acc.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateAccountInContestRequest.AccountId} Not Available!!!!!", "");
                }

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == request.CreateAccountInContestRequest.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Contest Id {request.CreateAccountInContestRequest.ContestId} Not Found!!!!!", "");
                if (contest.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest has already ended", "");

                if (acc.Coin < contest.CoinBetting)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin for this contest", "");

                acc.Coin -= contest.CoinBetting;

                var accountInContest = new AccountInContest();
                accountInContest.AccountId = acc.Id;
                accountInContest.ContestId = request.CreateAccountInContestRequest.ContestId;
                accountInContest.Contest = contest;
                await _unitOfWork.Repository<AccountInContest>().CreateAsync(accountInContest);

                //Update lại coin trong badge Tycoon của account nếu chưa đạt đủ 4000 coin
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == acc.Id && x.Challenge.Name.Equals("The Tycoon"));
                if (badge.CompletedDate == null && badge.CompletedLevel < badge.Challenge.CompletedMilestone)
                {
                    badge.CompletedLevel = (int)acc.Coin;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }

                await _unitOfWork.Repository<Account>().Update(acc, acc.Id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<AccountInContestResponse>(accountInContest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create account in contest error!!!", ex?.Message);
            }
        }
    }
}
