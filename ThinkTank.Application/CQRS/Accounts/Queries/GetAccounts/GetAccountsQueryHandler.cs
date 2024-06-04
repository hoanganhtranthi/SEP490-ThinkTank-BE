

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using AutoMapper.QueryableExtensions;

namespace ThinkTank.Application.Accounts.Queries.GetAccounts
{
    public class GetAccountsQueryHandler : IQueryHandler<GetAccountsQuery, PagedResults<AccountResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAccountsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<AccountResponse>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var filter = _mapper.Map<AccountResponse>(request.AccountRequest);
                var accounts = _unitOfWork.Repository<Account>().GetAll().AsNoTracking()
                                           .ProjectTo<AccountResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                foreach (var account in accounts)
                {
                    account.AmountReport = _unitOfWork.Repository<Report>().GetAll().Where(x => x.AccountId2 == account.Id).ToList().Count();
                }
                var sort = PageHelper<AccountResponse>.Sorting(request.PagingRequest.SortType, accounts, request.PagingRequest.ColName);
                var result = PageHelper<AccountResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get account list error!!!!!", ex.Message);
            }
        }
    }
}
