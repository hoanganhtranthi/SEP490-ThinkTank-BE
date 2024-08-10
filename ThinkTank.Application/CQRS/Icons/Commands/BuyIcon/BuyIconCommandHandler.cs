
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Icons.Commands.BuyIcon
{
    public class BuyIconCommandHandler : ICommandHandler<BuyIconCommand, IconOfAccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public BuyIconCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }
        public async Task<IconOfAccountResponse> Handle(BuyIconCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateIconOfAccountRequest.AccountId <= 0 || request.CreateIconOfAccountRequest.IconId <= 0 || request.CreateIconOfAccountRequest.AccountId == null || request.CreateIconOfAccountRequest.IconId == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                IconOfAccount iconOfAccount = _unitOfWork.Repository<IconOfAccount>()
                .Find(c => c.IconId == request.CreateIconOfAccountRequest.IconId && c.AccountId == request.CreateIconOfAccountRequest.AccountId);

                if (iconOfAccount != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateIconOfAccountRequest.AccountId} purchased this icon {request.CreateIconOfAccountRequest.IconId}", "");
                }

                Account account = _unitOfWork.Repository<Account>().Find(x => x.Id == request.CreateIconOfAccountRequest.AccountId);

                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {request.CreateIconOfAccountRequest.AccountId}", "");
                if (account.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {account.Id} Not Available!!!!!", "");
                }

                Icon icon = _unitOfWork.Repository<Icon>().Find(x => x.Id == request.CreateIconOfAccountRequest.IconId);
                if (icon == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found icon with id {request.CreateIconOfAccountRequest.IconId}", "");
                if (icon.Status == false)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Icon Id {request.CreateIconOfAccountRequest.IconId} is not available", "");

                var rs = _mapper.Map<CreateIconOfAccountRequest, IconOfAccount>(request.CreateIconOfAccountRequest);
                rs.IsAvailable = true;

                if (account.Coin < icon.Price)
                    throw new CrudException(HttpStatusCode.BadRequest, "Not enough coin to buy icon", "");

                account.Coin = account.Coin - icon.Price;

                await _unitOfWork.Repository<IconOfAccount>().CreateAsync(rs);

                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals("The Tycoon"));
                if (badge.CompletedDate == null && badge.CompletedLevel < badge.Challenge.CompletedMilestone)
                {
                    badge.CompletedLevel = (int)account.Coin;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }

                await _unitOfWork.Repository<Account>().Update(account, request.CreateIconOfAccountRequest.AccountId);
                await _unitOfWork.CommitAsync();

                var response = _mapper.Map<IconOfAccountResponse>(rs);
                response.UserName = account.UserName;
                response.IconAvatar = icon.Avatar;
                response.IconName = icon.Name;
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Buy Icon Error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Buy Icon Error!!!!!", ex.Message);
            }
        }
    }
}
