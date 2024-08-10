
using AutoMapper;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Accounts.Queries.GetAccountById
{
    public class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, AccountResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;

        public GetAccountByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }

        public async Task<AccountResponse> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _unitOfWork.Repository<Account>().GetAsync(u => u.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found account with id {request.Id}", "");
                }

                return _mapper.Map<AccountResponse>(response);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Account By ID Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
