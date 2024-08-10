
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Assets.Commands.DeleteAsset
{
    public class DeleteAssetCommandHandler : ICommandHandler<DeleteAssetCommand, List<AssetResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        public DeleteAssetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _slackService = slackService;
        }

        public async Task<List<AssetResponse>> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> assets = new List<Asset>();
                foreach (var id in request.Id)
                {
                    if (id <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _unitOfWork.Repository<Asset>().GetAll().Include(x => x.Topic).Include(x => x.Topic.Game).SingleOrDefault(x => x.Id == id);
                    if (asset == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Asset Id {id} is not found", "");

                    asset.Status = false;

                    var version = _unitOfWork.Repository<Asset>().GetAll().OrderBy(x => x.Version).LastOrDefault(x => x.Topic.GameId == asset.Topic.GameId).Version;
                    if (assets != null)
                    {
                        var assetOfGame = assets.SingleOrDefault(x => x.Topic.GameId == asset.Topic.GameId);
                        if (assetOfGame == null)
                        {
                            asset.Version = version + 1;
                            assets.Add(asset);
                        }
                        else
                        {
                            asset.Version = assetOfGame.Version;
                        }
                    }
                    rs = _mapper.Map<AssetResponse>(asset);
                    rs.TopicName = asset.Topic.Name;
                    rs.GameId = asset.Topic.GameId;
                    rs.GameName = asset.Topic.Game.Name;
                    result.Add(rs);

                    await _unitOfWork.Repository<Asset>().Update(asset, id);
                    await _unitOfWork.CommitAsync();
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Delete Asset Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Asset Error!!!", ex?.Message);
            }
        }
    }
}
