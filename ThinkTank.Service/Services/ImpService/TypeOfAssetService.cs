using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
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
    public class TypeOfAssetService : ITypeOfAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public TypeOfAssetService(IUnitOfWork unitOfWork, IMapper mapper,ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }
        public async Task<TypeOfAssetResponse> CreateTypeOfAsset(CreateTypeOfAssetRequest request)
        {
            try
            {

                var t = _mapper.Map<CreateTypeOfAssetRequest, TypeOfAsset>(request);
                var s = _unitOfWork.Repository<TypeOfAsset>().GetAll().Include(s=>s.Assets).SingleOrDefault(s => s.Type == request.Type);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Type Of Asset {request.Type} has already !!!", "");

                }
                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> list = new List<Asset>();
                foreach(var typeOfAsset in request.Assets)
                {
                    var topic = new Topic();                 
                    topic = _unitOfWork.Repository<Topic>().GetAll().Include(x => x.Game).SingleOrDefault(x => x.Id == typeOfAsset.TopicId);
                    if (topic == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"This topic {typeOfAsset.TopicId} is not found !!!", "");
                    var existingAsset = _unitOfWork.Repository<Asset>().Find(s => s.Value == typeOfAsset.Value && s.TopicId == typeOfAsset.TopicId);
                    if (existingAsset != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                    var asset = _mapper.Map<CreateAssetRequest, Asset>(typeOfAsset);
                    var expiryTime = DateTime.MaxValue;
                    var version = _cacheService.GetData<int>($"{topic.Game.Name}Version");
                    if (version != null)
                        _cacheService.SetData<int>($"{topic.Game.Name}Version", version += 1, expiryTime);
                    else version = 1;
                     rs = _mapper.Map<AssetResponse>(asset);
                    rs.TopicName = topic.Name;
                    rs.GameId = topic.Game.Id;
                    rs.GameName = topic.Game.Name;
                    result.Add(rs);
                    asset.Version = version;
                    asset.TopicId = topic.Id;
                    list.Add(asset);
                    t.Assets = list;
                }
                await _unitOfWork.Repository<TypeOfAsset>().CreateAsync(t);
                await _unitOfWork.CommitAsync();
                var response = _mapper.Map<TypeOfAssetResponse>(t);
                response.Assets = result.ToList();
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Type Of Asset Error!!!", ex?.Message);
            }
        }

        public async Task<TypeOfAssetResponse> DeleteTypeOfAsset(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Type Of Asset Invalid", "");
                }

                var response = _unitOfWork.Repository<TypeOfAsset>()
                    .GetAll()
                    .Include(x => x.Assets)
                    .SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset with id {id}", "");
                }

                foreach (var rs in response.Assets)
                {
                    var expiryTime = DateTime.MaxValue;
                    var topic = _unitOfWork.Repository<Topic>().GetAll().Include(x => x.Game).SingleOrDefault(x => x.Id == rs.TopicId);
                    var version = _cacheService.GetData<int>($"{topic.Game.Name}Version");
                    if (version != null)
                    {
                        _cacheService.SetData<int>($"{topic.Game.Name}Version", version += 1, expiryTime);
                    }
                    rs.Version = version;
                    version = 0;
                }
                _unitOfWork.Repository<Asset>().DeleteRange(response.Assets.ToArray());
                _unitOfWork.Repository<TypeOfAsset>().Delete(response);
                await _unitOfWork.CommitAsync();

                var result = new TypeOfAssetResponse
                {
                    Id = response.Id,
                    Type = response.Type,
                    Assets = response.Assets.Select(a => new AssetResponse
                    {
                        Id = a.Id,
                        TopicId = a.TopicId,
                        TopicName = a.Topic.Name,
                        GameId = a.Topic.GameId,
                        GameName = a.Topic.Game.Name,
                        Value = a.Value
                    }).ToList()
                };

                return result;

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Gettype of asset by id Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<TypeOfAssetResponse> GetTypeOfAssetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Type Of Asset Invalid", "");
                }
                var response = _unitOfWork.Repository<TypeOfAsset>().GetAll().Include(x => x.Assets).Select(x=>new TypeOfAssetResponse
                {
                    Id=x.Id,
                    Type=x.Type,
                    Assets=new List<AssetResponse>(x.Assets.Select(a=> new AssetResponse
                    {
                        Id=a.Id,
                        TopicId=a.TopicId,
                        TopicName=a.Topic.Name,
                        GameId=a.Topic.GameId,
                        GameName=a.Topic.Game.Name,
                        Value=a.Value
                    }))
                }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset with id {id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Gettype of asset by id Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<TypeOfAssetResponse>> GetTypeOfAssets(TypeOfAssetRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<TypeOfAssetResponse>(request);
                var typeOfAssetResponses = _unitOfWork.Repository<TypeOfAsset>().GetAll().Include(x => x.Assets)
                    .Select(x => new TypeOfAssetResponse
                    {
                        Id = x.Id,
                        Type=x.Type,
                        Assets=new List<AssetResponse>(x.Assets.Select(a=>new AssetResponse
                        {
                            Id = a.Id,
                            TopicId = a.TopicId,
                            TopicName = a.Topic.Name,
                            GameId = a.Topic.GameId,
                            GameName = a.Topic.Game.Name,
                            Value = a.Value,
                            Version=a.Version
                        }))
                    }).DynamicFilter(filter).ToList();
                if (request.GameId != null)
                {
                    typeOfAssetResponses = typeOfAssetResponses
                        .Where(asset => asset.Assets.Any(a => a.GameId == request.GameId))
                        .ToList();
                }
                if (request.TopicId!= null)
                    typeOfAssetResponses = typeOfAssetResponses.Where(asset => asset.Assets.Any(x => x.TopicId == request.TopicId)).ToList();
                if(request.Version != null)
                    typeOfAssetResponses = typeOfAssetResponses
                       .Where(asset => asset.Assets.Any(a => a.Version > request.Version))
                       .ToList();
                var sort = PageHelper<TypeOfAssetResponse>.Sorting(paging.SortType, typeOfAssetResponses, paging.ColName);
                var result = PageHelper<TypeOfAssetResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of assets list error!!!!!", ex.Message);
            }
        }
        public async Task<TypeOfAssetResponse> UpdateTypeOfAsset(int id, CreateTypeOfAssetRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Asset Invalid", "");
                }
                TypeOfAsset asset = _unitOfWork.Repository<TypeOfAsset>().GetAll().Include(x=>x.Assets)
                    .SingleOrDefault(c => c.Id == id);
                if (asset == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"id {id} not found", "");
                _unitOfWork.Repository<Asset>().DeleteRange(asset.Assets.ToArray());
                _mapper.Map<CreateTypeOfAssetRequest, TypeOfAsset>(request, asset);
                var s = _unitOfWork.Repository<TypeOfAsset>().GetAll().Include(s => s.Assets).SingleOrDefault(s => s.Type == request.Type && s.Id != id);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Type Of Asset {request.Type} has already !!!", "");

                }
                AssetResponse rs = new AssetResponse();
                List<AssetResponse> result = new List<AssetResponse>();
                List<Asset> list = new List<Asset>();
                foreach (var typeOfAsset in request.Assets)
                {
                    var topic = new Topic();
                    topic = _unitOfWork.Repository<Topic>().GetAll().Include(x => x.Game).SingleOrDefault(x => x.Id == typeOfAsset.TopicId );
                    if (topic == null)
                        throw new CrudException(HttpStatusCode.NotFound, $"This topic {typeOfAsset.TopicId} is not found !!!", "");
                    var existingAsset = _unitOfWork.Repository<Asset>().Find(s => s.Value == typeOfAsset.Value && s.TopicId == typeOfAsset.TopicId && s.TypeOfAssetId!=id);
                    if (existingAsset != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                    var a = _mapper.Map<CreateAssetRequest, Asset>(typeOfAsset);
                    a.TypeOfAssetId =  id;
                        var expiryTime = DateTime.MaxValue;
                    var version = _cacheService.GetData<int>($"{topic.Game.Name}Version");
                    if (version != null)
                    {
                        if (asset.Assets.SingleOrDefault(x => x.TopicId == typeOfAsset.TopicId && x.Value == typeOfAsset.Value) == null)
                        {
                            var v = version + 1;
                            _cacheService.SetData<int>($"{topic.Game.Name}Version", v, expiryTime);
                        }
                    }
                    a.Version = version;
                    rs = _mapper.Map<AssetResponse>(a);
                    rs.TopicName = topic.Name;
                    rs.GameId = topic.Game.Id;
                    rs.GameName = topic.Game.Name;
                    result.Add(rs);
                    a.TopicId = topic.Id;
                    list.Add(a);
                    asset.Assets = list;
                    version = 0;
                }
                await _unitOfWork.Repository<TypeOfAsset>().Update(asset,id);
                await _unitOfWork.CommitAsync();
                var response = _mapper.Map<TypeOfAssetResponse>(asset);
                response.Assets = result.ToList();
                return response;
            }
            catch(CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update type of asset error !!!", ex.Message);
            }

        }
    }
}
