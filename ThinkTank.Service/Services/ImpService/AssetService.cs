using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Get asset by id Error!!!", ex.InnerException?.Message);
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
                List<Asset> gameId = new List<Asset>();
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
                   
                    var asset = _mapper.Map<CreateAssetRequest, Asset>(a);
                   
                    var version = _unitOfWork.Repository<Asset>().GetAll()
                        .OrderBy(x => x.Version).LastOrDefault(x => x.Topic.GameId == topic.GameId);
                    
                    if (gameId != null)
                    {
                        var game = gameId.SingleOrDefault(x => x.Topic.GameId == topic.GameId);
                        if (game == null)
                        {
                            if (version == null) asset.Version = 1;
                            else asset.Version = version.Version + 1;
                            gameId.Add(asset);
                        }                           
                        else
                        {
                            asset.Version = game.Version;
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
                List<Asset> gameId = new List<Asset>();
                foreach (var a in request)
                {
                    if (a <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                    var asset = _unitOfWork.Repository<Asset>().GetAll().Include(x => x.Topic).Include(x => x.Topic.Game).SingleOrDefault(x => x.Id == a);
                    if (asset == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"Asset Id {a} is not found","");
                    asset.Status = false;
                    var version = _unitOfWork.Repository<Asset>().GetAll().OrderBy(x => x.Version).LastOrDefault(x => x.Topic.GameId == asset.Topic.GameId).Version;                    
                    if (gameId != null)
                    {
                        var game = gameId.SingleOrDefault(x => x.Topic.GameId == asset.Topic.GameId);
                        if (game == null)
                        {
                            asset.Version = version + 1;
                            gameId.Add(asset);
                        }
                        else
                        {
                            asset.Version = game.Version;
                        }
                    }
                    rs = _mapper.Map<AssetResponse>(asset);
                    rs.TopicName = asset.Topic.Name;
                    rs.GameId = asset.Topic.GameId;
                    rs.GameName = asset.Topic.Game.Name;
                    result.Add(rs);
                    await _unitOfWork.Repository<Asset>().Update(asset,a);
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
