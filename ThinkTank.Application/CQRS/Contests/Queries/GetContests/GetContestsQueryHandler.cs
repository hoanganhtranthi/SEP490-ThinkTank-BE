

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetContests
{
    public class GetContestsQueryHandler : IQueryHandler<GetContestsQuery, PagedResults<ContestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetContestsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<ContestResponse>> Handle(GetContestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request.ContestRequest);
                var contests = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().
                    Include(x => x.AssetOfContests).Include(x => x.Game)
                                           .Select(x => new ContestResponse
                                           {
                                               Id = x.Id,
                                               EndTime = x.EndTime,
                                               StartTime = x.StartTime,
                                               AssetOfContests = _mapper.Map<List<AssetOfContestResponse>>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                                               {
                                                   ContestId = a.ContestId,
                                                   Id = a.Id,
                                                   Value = a.Value,
                                                   NameOfContest = x.Name,
                                                   Answer = x.GameId == 2 ? System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).LastIndexOf('.')) : null,
                                                   Type = a.TypeOfAsset.Type
                                               })),
                                               Name = x.Name,
                                               Status = x.Status,
                                               Thumbnail = x.Thumbnail,
                                               GameId = x.GameId,
                                               PlayTime = x.PlayTime,
                                               CoinBetting = x.CoinBetting,
                                               GameName = x.Game.Name,
                                               AmoutPlayer = x.AccountInContests.Count()
                                           })
                                           .DynamicFilter(filter)
                                           .ToList();
                if (request.ContestRequest.ContestStatus != StatusType.All)
                {
                    bool? status = null;
                    if (request.ContestRequest.ContestStatus.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.ContestRequest.ContestStatus.ToString().ToLower());
                    }
                    contests = contests.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<ContestResponse>.Sorting(request.PagingRequest.SortType, contests, request.PagingRequest.ColName);
                var result = PageHelper<ContestResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get contest list error!!!!!", ex.Message);
            }
        }
    }
}
