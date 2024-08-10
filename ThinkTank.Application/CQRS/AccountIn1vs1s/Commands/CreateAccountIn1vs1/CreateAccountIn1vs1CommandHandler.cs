
using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateAccountIn1vs1
{
    public class CreateAccountIn1vs1CommandHandler : ICommandHandler<CreateAccountIn1vs1Command, AccountIn1vs1Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public readonly IBadgesService _badgesService;
        private readonly DateTime date;
        private readonly ISlackService _slackService;
        public CreateAccountIn1vs1CommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBadgesService badgesService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _badgesService = badgesService;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _slackService = slackService;
        }

        public async Task<AccountIn1vs1Response> Handle(CreateAccountIn1vs1Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateAndUpdateAccountIn1Vs1Request.AccountId1 <= 0 || request.CreateAndUpdateAccountIn1Vs1Request.AccountId2 <= 0 || request.CreateAndUpdateAccountIn1Vs1Request.Coin <= 0
                    || request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id == null || request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id == "" || request.CreateAndUpdateAccountIn1Vs1Request.WinnerId < 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var accIn1vs1 = _mapper.Map<CreateAndUpdateAccountIn1vs1Request, AccountIn1vs1>(request.CreateAndUpdateAccountIn1Vs1Request);

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
                var c = _unitOfWork.Repository<Game>().GetAll().SingleOrDefault(c => c.Id == request.CreateAndUpdateAccountIn1Vs1Request.GameId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, "Game Not Found!!!!!", "");
                }

                var accountIn1vs1 = _unitOfWork.Repository<AccountIn1vs1>().Find(x => x.AccountId1 == request.CreateAndUpdateAccountIn1Vs1Request.AccountId1 && x.AccountId2 == request.CreateAndUpdateAccountIn1Vs1Request.AccountId2 && x.RoomOfAccountIn1vs1Id == request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id);
                if (accountIn1vs1 != null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"These two accounts have already played against this room id {request.CreateAndUpdateAccountIn1Vs1Request.RoomOfAccountIn1vs1Id} together", "");

                accIn1vs1.StartTime = date;
                accIn1vs1.Game = c;
                accIn1vs1.AccountId1Navigation = account1;
                accIn1vs1.AccountId2Navigation = account2;

                if (account1.Coin < request.CreateAndUpdateAccountIn1Vs1Request.Coin)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Not enough coin for this 1vs1 of account Id {account1.Id}", "");
                account1.Coin -= request.CreateAndUpdateAccountIn1Vs1Request.Coin;
                if (account2.Coin < request.CreateAndUpdateAccountIn1Vs1Request.Coin)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Not enough coin for this 1vs1 of account Id {account2.Id}", "");
                account2.Coin -= request.CreateAndUpdateAccountIn1Vs1Request.Coin;

                await _unitOfWork.Repository<AccountIn1vs1>().CreateAsync(accIn1vs1);
                await _unitOfWork.Repository<Account>().Update(account1, account1.Id);
                await _unitOfWork.Repository<Account>().Update(account2, account2.Id);

                await _badgesService.GetBadge(account1, "The Tycoon");
                await _badgesService.GetBadge(account2, "The Tycoon");
                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<AccountIn1vs1Response>(accIn1vs1);
                rs.GameName = c.Name;
                rs.Username1 = account1.UserName;
                rs.Username2 = account2.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Create Account In 1vs1 Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Account In 1vs1 Error!!!", ex?.Message);
            }
        }
    }
}
