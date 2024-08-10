

using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetAccountInContests
{
    public class GetAccountInContestsQueryHandler : IQueryHandler<GetAccountInContestsQuery, PagedResults<AccountInContestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public GetAccountInContestsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper,ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }

        public async Task<PagedResults<AccountInContestResponse>> Handle(GetAccountInContestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var filter = _mapper.Map<AccountInContestResponse>(request.AccountInContestRequest);
                var accountInContests = _unitOfWork.Repository<AccountInContest>().GetAll().AsNoTracking()
                .Include(x => x.Account).Include(x => x.Contest)
                .Select(x => new AccountInContestResponse
                {
                    Id = x.Id,
                    UserName = x.Account.UserName,
                    ContestName = x.Contest.Name,
                    CompletedTime = x.CompletedTime,
                    Duration = x.Duration,
                    AccountId = x.AccountId,
                    ContestId = x.ContestId,
                    Mark = x.Mark,
                    Avatar = x.Account.Avatar,
                    Prize = x.Prize
                }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AccountInContestResponse>.Sorting(request.PagingRequest.SortType, accountInContests, request.PagingRequest.ColName);
                var result = PageHelper<AccountInContestResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Account In Contest list error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Account In Contest list error!!!!!", ex.Message);
            }
        }
    }
}
