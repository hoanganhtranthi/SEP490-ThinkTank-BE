

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Commands.UpdateAccountInContest
{
    public class UpdateAccountInContestCommandHandler : ICommandHandler<UpdateAccountInContestCommand, AccountInContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IBadgesService _badgesService;
        public UpdateAccountInContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBadgesService badgesService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _badgesService = badgesService;
        }

        public async Task<AccountInContestResponse> Handle(UpdateAccountInContestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.UpdateAccountInContestRequest.ContestId <= 0 || request.UpdateAccountInContestRequest.AccountId <= 0 || request.UpdateAccountInContestRequest.Duration < 0 || request.UpdateAccountInContestRequest.Mark < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var accountInContest = _unitOfWork.Repository<AccountInContest>()
                    .Find(s => s.ContestId == request.UpdateAccountInContestRequest.ContestId && s.AccountId == request.UpdateAccountInContestRequest.AccountId);
                if (accountInContest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Account in Contest is not found !!!", "");
                }

                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == request.UpdateAccountInContestRequest.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Account Not Found!!!!!", "");
                }
                if (a.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Account Not Available!!!!!", "");
                }

                var c = _unitOfWork.Repository<Contest>().Find(c => c.Id == request.UpdateAccountInContestRequest.ContestId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Contest Not Found!!!!!", "");
                }
                if (c.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest Not Available!!!!!", "");
                }

                _mapper.Map<UpdateAccountInContestRequest, AccountInContest>(request.UpdateAccountInContestRequest, accountInContest);
                accountInContest.Prize = request.UpdateAccountInContestRequest.Mark / 10;
                accountInContest.CompletedTime = date;
                a.Coin += accountInContest.Prize;

                await _unitOfWork.Repository<AccountInContest>().Update(accountInContest, accountInContest.Id);
                await _unitOfWork.Repository<Account>().Update(a, request.UpdateAccountInContestRequest.AccountId);

                //Get badge
                await _badgesService.GetBadge(a, "The Tycoon");
                await _badgesService.GetBadge(a, "Super enthusiastic");

                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AccountInContestResponse>(accountInContest);
                rs.ContestName = c.Name;
                rs.UserName = a.UserName;
                rs.Avatar = a.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Account In Contest Error!!!", ex?.Message);
            }
        }
    }
}
