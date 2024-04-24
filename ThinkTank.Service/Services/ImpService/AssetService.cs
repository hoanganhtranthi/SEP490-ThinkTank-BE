using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AssetResponse> GetAssetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Type Of Asset Invalid", "");
                }
                var response = _unitOfWork.Repository<Asset>().GetAll().AsNoTracking()
                    .Include(x => x.Topic).Include(x=>x.Topic.Game).Select(x => new AssetResponse
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
                    }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found asset with id {id}", "");
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

        public async Task<PagedResults<AssetResponse>> GetAssets(AssetRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<AssetResponse>(request);
                var assetResponses = _unitOfWork.Repository<Asset>().GetAll().AsNoTracking().Include(x => x.Topic).Include(x => x.Topic.Game).Select(x => new AssetResponse
                {
                    Id = x.Id,
                    TopicId = x.TopicId,
                    TopicName = x.Topic.Name,
                    Status = x.Status,
                    Version=x.Version,
                    Answer = x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(x.Value).LocalPath).LastIndexOf('.')) : null,
                    GameId = x.Topic.GameId,
                    GameName = x.Topic.Game.Name,
                    Value = x.Value
                }).DynamicFilter(filter).ToList();
                if (request.Version != null)
                    assetResponses = assetResponses.Where(x => x.Version > request.Version).ToList();
                var sort = PageHelper<AssetResponse>.Sorting(paging.SortType, assetResponses, paging.ColName);
                var result = PageHelper<AssetResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get assets list error!!!!!", ex.Message);
            }
        }
        public async Task<List<AssetResponse>> CreateAsset(List<CreateAssetRequest> request)
        {
            try
            {

                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> assets = new List<Asset>();
                foreach (var a in request)
                {
                    if (a.TopicId <= 0 || a.TypeOfAssetId <=0 || a.Value==null || a.Value=="")
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
        public async Task<List<AssetResponse>> DeleteAsset(List<int> request)
        {
            try
            {
                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> assets = new List<Asset>();
                foreach (var id in request)
                {
                    if (id <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _unitOfWork.Repository<Asset>().GetAll().Include(x => x.Topic).Include(x => x.Topic.Game).SingleOrDefault(x => x.Id == id);
                    if (asset == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Asset Id {id} is not found","");

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

                    await _unitOfWork.Repository<Asset>().Update(asset,id);
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Asset Error!!!", ex?.Message);
            }
        }
    }
}
