

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Assets.Queries.GetAssetById
{
    public class GetAssetByIdQueyHandler : IQueryHandler<GetAssetByIdQuery, AssetResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAssetByIdQueyHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AssetResponse> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Asset>().GetAll().AsNoTracking()
                    .Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new AssetResponse
                    {
                        Id = x.Id,
                        TopicId = x.TopicId,
                        TopicName = x.Topic.Name,
                        GameId = x.Topic.GameId,
                        Status = x.Status,
                        Version = x.Version,
                        Answer = x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).LastIndexOf('.')) : null,
                        GameName = x.Topic.Game.Name,
                        Value = x.Value
                    }).SingleOrDefault(x => x.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found asset with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get asset by id error!!!", ex.InnerException?.Message);
            }
        }
    }
}
