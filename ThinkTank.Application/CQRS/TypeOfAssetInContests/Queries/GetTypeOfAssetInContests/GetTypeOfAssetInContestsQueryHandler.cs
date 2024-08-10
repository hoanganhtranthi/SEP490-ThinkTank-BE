

using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContests
{
    public class GetTypeOfAssetInContestsQueryHandler : IQueryHandler<GetTypeOfAssetInContestsQuery, PagedResults<TypeOfAssetInContestResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetTypeOfAssetInContestsQueryHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<PagedResults<TypeOfAssetInContestResponse>> Handle(GetTypeOfAssetInContestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var typeOfAssetResponses = _unitOfWork.Repository<TypeOfAssetInContest>().GetAll().AsNoTracking().Include(x => x.AssetOfContests)
                    .Select(x => new TypeOfAssetInContestResponse
                    {
                        Id = x.Id,
                        Type = x.Type,
                        AssetOfContests = new List<AssetOfContestResponse>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                        {
                            Id = a.Id,
                            ContestId = a.ContestId,
                            NameOfContest = a.Contest.Name,
                            Type = a.TypeOfAsset.Type,
                            Value = a.Value
                        }))
                    }).ToList();

                if (request.ContestId != null)
                {
                    typeOfAssetResponses = typeOfAssetResponses
                    .Where(asset => asset.AssetOfContests.Any(a => a.ContestId == request.ContestId))
                        .ToList();
                }
                var sort = PageHelper<TypeOfAssetInContestResponse>.Sorting(request.PagingRequest.SortType, typeOfAssetResponses, request.PagingRequest.ColName);
                var result = PageHelper<TypeOfAssetInContestResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get type of assets in contest list error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of assets in contest list error!!!!!", ex.Message);
            }
        }
    }
}
