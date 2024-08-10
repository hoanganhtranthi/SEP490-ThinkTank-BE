

using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.UpdateAccount1vs1
{
    public class UpdateAccount1vs1CommandHandler : ICommandHandler<UpdateAccount1vs1Command, AccountIn1vs1Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public readonly IBadgesService _badgesService;
        private readonly DateTime date;
        private readonly ISlackService _slackService;
        public UpdateAccount1vs1CommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBadgesService badgesService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _badgesService = badgesService;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _slackService = slackService;
        }

        public async Task<AccountIn1vs1Response> Handle(UpdateAccount1vs1Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAndUpdateAccountIn1Vs1Request.AccountId1 <= 0 || request.CreateAndUpdateAccountIn1Vs1Request.AccountId2 <= 0 || request.CreateAndUpdateAccountIn1Vs1Request.Coin <= 0
                    || request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id == null || request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id == "" || request.CreateAndUpdateAccountIn1Vs1Request.WinnerId < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var accIn1vs1 = _unitOfWork.Repository<AccountIn1vs1>().Find(x => x.AccountId1 == request.CreateAndUpdateAccountIn1Vs1Request.AccountId1 && x.AccountId2 == request.CreateAndUpdateAccountIn1Vs1Request.AccountId2
                && x.RoomOfAccountIn1vs1Id == request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id);

                if (accIn1vs1 == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId1} and account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId2} in 1vs1 in room {request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id} is not found", "");

                if (accIn1vs1.EndTime != null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account 1vs1 has already winner, you can not update", "");

                _mapper.Map<CreateAndUpdateAccountIn1vs1Request, AccountIn1vs1>(request.CreateAndUpdateAccountIn1Vs1Request, accIn1vs1);

                if (request.CreateAndUpdateAccountIn1Vs1Request.WinnerId != 0 && request.CreateAndUpdateAccountIn1Vs1Request.WinnerId != request.CreateAndUpdateAccountIn1Vs1Request.AccountId1 && request.CreateAndUpdateAccountIn1Vs1Request.WinnerId != request.CreateAndUpdateAccountIn1Vs1Request.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Winner Id {request.CreateAndUpdateAccountIn1Vs1Request.WinnerId} is invalid !!!!!", "");

                accIn1vs1.EndTime = date;

                var account1 = _unitOfWork.Repository<Account>().Find(a => a.Id == request.CreateAndUpdateAccountIn1Vs1Request.AccountId1);
                if (account1 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId1} Not Found!!!!!", "");
                }
                if (account1.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId1} Not Available!!!!!", "");
                }
                var account2 = _unitOfWork.Repository<Account>().Find(a => a.Id == request.CreateAndUpdateAccountIn1Vs1Request.AccountId2);
                if (account2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId2} Not Found!!!!!", "");
                }
                if (account2.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateAndUpdateAccountIn1Vs1Request.AccountId2}  Not Available!!!!!", "");
                }

                if (request.CreateAndUpdateAccountIn1Vs1Request.WinnerId == account1.Id)
                {
                    account1.Coin += (request.CreateAndUpdateAccountIn1Vs1Request.Coin * 2);
                    await _badgesService.GetBadge(account1, "Athlete");
                }

                else if (request.CreateAndUpdateAccountIn1Vs1Request.WinnerId == account2.Id)
                {
                    account2.Coin += request.CreateAndUpdateAccountIn1Vs1Request.Coin * 2;
                    await _badgesService.GetBadge(account2, "Athlete");
                }

                else if (request.CreateAndUpdateAccountIn1Vs1Request.WinnerId == 0 || request.CreateAndUpdateAccountIn1Vs1Request.WinnerId == null)
                {
                    account1.Coin += request.CreateAndUpdateAccountIn1Vs1Request.Coin;
                    account2.Coin += request.CreateAndUpdateAccountIn1Vs1Request.Coin;
                }

                await _unitOfWork.Repository<AccountIn1vs1>().Update(accIn1vs1, accIn1vs1.Id);
                await _unitOfWork.Repository<Account>().Update(account1, account1.Id);
                await _unitOfWork.Repository<Account>().Update(account2, account2.Id);

                await _badgesService.GetBadge(account1, "The Tycoon");
                await _badgesService.GetBadge(account2, "The Tycoon");
                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<AccountIn1vs1Response>(accIn1vs1);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Update Account In 1vs1 Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Account In 1vs1 Error!!!", ex?.Message);
            }
        }
    }
}
