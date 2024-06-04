

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.TypeOfAssets.Queries.GetTypeOfAssets
{
    public class GetTypeOfAssetsQueryHandler : IQueryHandler<GetTypeOfAssetsQuery, PagedResults<TypeOfAssetResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetTypeOfAssetsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResults<TypeOfAssetResponse>> Handle(GetTypeOfAssetsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var typeOfAssetResponses = _unitOfWork.Repository<TypeOfAsset>().GetAll().AsNoTracking().Include(x => x.Assets)
                    .Select(x => new TypeOfAssetResponse
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Assets = new List<AssetResponse>(x.Assets.Select(a => new AssetResponse
                        {
                            Id = a.Id,
                            TopicId = a.TopicId,
                            TopicName = a.Topic.Name,
                            GameId = a.Topic.GameId,
                            GameName = a.Topic.Game.Name,
                            Status = a.Status,
                            Value = a.Value,
                            Version = a.Version
                        }))
                    }).ToList();

                var sort = PageHelper<TypeOfAssetResponse>.Sorting(request.PagingRequest.SortType, typeOfAssetResponses, request.PagingRequest.ColName);
                var result = PageHelper<TypeOfAssetResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of assets list error!!!!!", ex.Message);
            }
        }
    }
}
