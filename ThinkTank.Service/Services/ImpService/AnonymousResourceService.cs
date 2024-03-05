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
    public class AnonymousResourceService : IAnonymousResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public AnonymousResourceService(IUnitOfWork unitOfWork, IMapper mapper,ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<AnonymousResponse> CreateAnonymousResource(AnonymousRequest createAnonymousRequest)
        {
            try
            {
                var anonymous = _mapper.Map<AnonymousRequest, Anonymous>(createAnonymousRequest);
                var s = _unitOfWork.Repository<Anonymous>().Find(s => s.Description==createAnonymousRequest.Description && s.Characteristic==createAnonymousRequest.Characteristic);
                if (s != null)           
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                if (createAnonymousRequest.Characteristic <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Characteristic is invalid", "");

                var topic = _unitOfWork.Repository<TopicOfGame>().GetAll().Include(x => x.Topic).SingleOrDefault(x => x.Id == anonymous.TopicOfGameId);
                if(topic== null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic of game {createAnonymousRequest.TopicOfGameId} is not found !!!", "");

                await _unitOfWork.Repository<Anonymous>().CreateAsync(anonymous);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymousVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymousVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<AnonymousResponse>(anonymous);
                rs.TopicName = topic.Topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Anonymous Resource Error!!!", ex?.Message);
            }
        }
 
        public async Task<AnonymousResponse> GetAnonymousResourceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<Anonymous>().GetAll().Include(x=>x.TopicOfGame.Topic).SingleOrDefault(x=>x.Id==id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AnonymousResponse>(response);
                rs.TopicName = response.TopicOfGame.Topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Resource By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AnonymousResponse>> GetAnonymousResources(ResourceRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<AnonymousResponse>(request);
                var friends = _unitOfWork.Repository<Anonymous>().GetAll().Include(x=>x.TopicOfGame).Include(x=>x.TopicOfGame.Topic)
                    .Select(x => new AnonymousResponse
                    {
                        Id = x.Id,
                        TopicOfGameId = x.TopicOfGameId,
                        Characteristic=x.Characteristic,
                        Description=x.Description,
                        LinkImg=x.LinkImg,
                        TopicName=x.TopicOfGame.Topic.Name,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AnonymousResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<AnonymousResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<AnonymousResponse> UpdateAnonymousResource(int id, AnonymousRequest request)
        {
            try
            {
               Anonymous anonymous = _unitOfWork.Repository<Anonymous>()
                     .Find(c => c.Id == id);

                if (anonymous== null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found anonymous resource with id{id.ToString()}", "");

                if (anonymous.Characteristic <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Characteristic is invalid", "");

                var s = _unitOfWork.Repository<Anonymous>().Find(s => s.Description == request.Description && s.Characteristic == request.Characteristic && s.Id != anonymous.Id);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var topic = _unitOfWork.Repository<TopicOfGame>().Find(x => x.Id == anonymous.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic of game {request.TopicOfGameId} is not found !!!", "");
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymousVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymousVersion", version+=1, expiryTime);
                _mapper.Map<AnonymousRequest,Anonymous>(request, anonymous);                
                await _unitOfWork.Repository<Anonymous>().Update(anonymous, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AnonymousResponse>(anonymous);
                rs.TopicName = _unitOfWork.Repository<Topic>().Find(x => x.Id == rs.TopicOfGameId).Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update anonymous resource error!!!!!", ex.Message);
            }
        }
        public async Task<AnonymousResponse> DeleteAnonymousResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<Anonymous>().GetAll().Include(x=>x.TopicOfGame.Topic).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<Anonymous>().Delete(response);
              await  _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymousVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymousVersion", version += 1, expiryTime);

                var rs = _mapper.Map<AnonymousResponse>(response);
                rs.TopicName = response.TopicOfGame.Topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Resource  Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
