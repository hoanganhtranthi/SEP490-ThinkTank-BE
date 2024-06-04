

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

namespace ThinkTank.Application.CQRS.Assets.Queries.GetAssets
{
    public class GetAssetsQueryHandler : IQueryHandler<GetAssetsQuery, PagedResults<AssetResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAssetsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<AssetResponse>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var filter = _mapper.Map<AssetResponse>(request.AssetRequest);
                var assetResponses = _unitOfWork.Repository<Asset>().GetAll().AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new AssetResponse
                {
                    Id = x.Id,
                    TopicId = x.TopicId,
                    TopicName = x.Topic.Name,
                    Status = x.Status,
                    Version = x.Version,
                    Answer = x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).LastIndexOf('.')) : null,
                    GameId = x.Topic.GameId,
                    GameName = x.Topic.Game.Name,
                    Value = x.Value
                }).DynamicFilter(filter).ToList();
                if (request.AssetRequest.Version != null)
                    assetResponses = assetResponses.Where(x => x.Version > request.AssetRequest.Version).ToList();
                var sort = PageHelper<AssetResponse>.Sorting(request.PagingRequest.SortType, assetResponses, request.PagingRequest.ColName);
                var result = PageHelper<AssetResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get assets list error!!!!!", ex.Message);
            }
        }
    }
}
