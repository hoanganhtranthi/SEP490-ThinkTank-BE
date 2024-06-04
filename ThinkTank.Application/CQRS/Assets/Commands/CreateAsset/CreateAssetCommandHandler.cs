

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Assets.Commands.CreateAsset
{
    public class CreateAssetCommandHandler : ICommandHandler<CreateAssetCommand, List<AssetResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CreateAssetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<AssetResponse>> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
        {
            try
            {

                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> assets = new List<Asset>();
                foreach (var a in request.AssetRequests)
                {
                    if (a.TopicId <= 0 || a.TypeOfAssetId <= 0 || a.Value == null || a.Value == "")
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var topic = _unitOfWork.Repository<Topic>().GetAll().Include(x => x.Game).SingleOrDefault(x => x.Id == a.TopicId);
                    if (topic == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"This topic {a.TopicId} is not found !!!", "");

                    var existingAsset = _unitOfWork.Repository<Asset>().Find(s => s.Value == a.Value && s.TopicId == a.TopicId);
                    if (existingAsset != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                    var typeOfAsset = _unitOfWork.Repository<TypeOfAsset>().Find(x => x.Id == a.TypeOfAssetId);
                    if (typeOfAsset == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"This type of asset {a.TypeOfAssetId} is not found !!!", "");

                    if (topic.Game.Name.Equals("Flip Card") || topic.Game.Name.Equals("Images Walkthrough"))
                    {
                        if (typeOfAsset.Type.Equals("Description+ImgLink") || typeOfAsset.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset Invalid!!!!!", "");
                        }
                        else
                        {
                            if (a.Value.Contains(";") || a.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset  Invalid!!!!!", "");
                            }
                        }
                    }
                    else if (topic.Game.Name.Equals("Music Password"))
                    {
                        if (typeOfAsset.Type.Equals("Description+ImgLink") || typeOfAsset.Type.Equals("ImgLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!a.Value.Contains(".mp3"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset Invalid!!!!!", "");
                            }
                        }
                    }
                    else
                    {
                        if (typeOfAsset.Type.Equals("ImgLink") || typeOfAsset.Type.Equals("AudioLink"))
                        {
                            throw new CrudException(HttpStatusCode.NotFound, "Type Of Asset  Invalid!!!!!", "");
                        }
                        else
                        {
                            if (!a.Value.Contains(";"))
                            {
                                throw new CrudException(HttpStatusCode.NotFound, "Asset Invalid!!!!!", "");
                            }
                        }
                    }

                    var asset = _mapper.Map<CreateAssetRequest, Asset>(a);

                    var asset1 = _unitOfWork.Repository<Asset>().GetAll()
                        .OrderBy(x => x.Version).LastOrDefault(x => x.Topic.GameId == topic.GameId);

                    if (assets != null)
                    {
                        var assestOfGame = assets.SingleOrDefault(x => x.Topic.GameId == topic.GameId);
                        if (assestOfGame == null)
                        {
                            if (asset1 == null) asset.Version = 1;
                            else asset.Version = asset1.Version + 1;
                            assets.Add(asset);
                        }
                        else
                        {
                            asset.Version = assestOfGame.Version;
                        }
                    }
                    asset.TopicId = topic.Id;
                    asset.TypeOfAssetId = typeOfAsset.Id;
                    asset.Status = true;
                    rs = _mapper.Map<AssetResponse>(asset);
                    rs.TopicName = topic.Name;
                    rs.GameId = topic.Game.Id;
                    rs.GameName = topic.Game.Name;
                    result.Add(rs);

                    await _unitOfWork.Repository<Asset>().CreateAsync(asset);
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Asset Error!!!", ex?.Message);
            }
        }
    }
}
