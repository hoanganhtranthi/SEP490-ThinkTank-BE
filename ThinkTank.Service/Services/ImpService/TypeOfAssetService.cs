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
        public TypeOfAssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
           
        public async Task<TypeOfAssetResponse> GetTypeOfAssetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Type Of Asset Invalid", "");
                }
                var response = _unitOfWork.Repository<TypeOfAsset>().GetAll().AsNoTracking().Include(x => x.Assets).Select(x=>new TypeOfAssetResponse
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
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset with id {id}", "");
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

        public async Task<PagedResults<TypeOfAssetResponse>> GetTypeOfAssets(TypeOfAssetRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<TypeOfAssetResponse>(request);
                var typeOfAssetResponses = _unitOfWork.Repository<TypeOfAsset>().GetAll().AsNoTracking().Include(x => x.Assets)
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
                            Status=a.Status,
                            Value = a.Value,
                            Version=a.Version
                        }))
                    }).DynamicFilter(filter).ToList();

                var sort = PageHelper<TypeOfAssetResponse>.Sorting(paging.SortType, typeOfAssetResponses, paging.ColName);
                var result = PageHelper<TypeOfAssetResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of assets list error!!!!!", ex.Message);
            }
        }

    }
}
