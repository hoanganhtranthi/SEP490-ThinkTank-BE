

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.TypeOfAssets.Queries.GetTypeOfAssetById
{
    public class GetTypeOfAssetByIdQueryHanlder : IQueryHandler<GetTypeOfAssetByIdQuery, TypeOfAssetResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetTypeOfAssetByIdQueryHanlder(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TypeOfAssetResponse> Handle(GetTypeOfAssetByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<TypeOfAsset>().GetAll().AsNoTracking().Include(x => x.Assets).Select(x => new TypeOfAssetResponse
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
                        Value = a.Value
                    }))
                }).SingleOrDefault(x => x.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of asset by id Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
