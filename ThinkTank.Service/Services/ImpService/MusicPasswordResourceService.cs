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
    public class MusicPasswordResourceService : IMusicPasswordResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public MusicPasswordResourceService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<MusicPasswordResponse> CreateMusicPasswordResource(MusicPasswordRequest createMusicPasswordRequest)
        {
            try
            {
                var musicPassword = _mapper.Map<MusicPasswordRequest, MusicPassword>(createMusicPasswordRequest);
                var s = _unitOfWork.Repository<MusicPassword>().Find(s => s.Password == createMusicPasswordRequest.Password);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
              
                var topic = _unitOfWork.Repository<Topic>().Find(x => x.Id == musicPassword.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic {createMusicPasswordRequest.TopicOfGameId} is not found !!!", "");

                await _unitOfWork.Repository<MusicPassword>().CreateAsync(musicPassword);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<MusicPasswordResponse>(musicPassword);
                rs.TopicName = topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Music Password Resource Error!!!", ex?.Message);
            }
        }

        public async Task<MusicPasswordResponse> DeleteMusicPasswordResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<MusicPassword>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<MusicPassword>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordVersion", version += 1, expiryTime);

                var rs = _mapper.Map<MusicPasswordResponse>(response);
                rs.TopicName = _unitOfWork.Repository<Topic>().Find(x => x.Id == rs.TopicOfGameId).Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Resource By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<MusicPasswordResponse> GetMusicPasswordResourceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<MusicPassword>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<MusicPasswordResponse>(response);
                rs.TopicName = _unitOfWork.Repository<Topic>().Find(x => x.Id == rs.TopicOfGameId).Name;
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

        public async Task<PagedResults<MusicPasswordResponse>> GetMusicPasswordResources(ResourceRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<MusicPasswordResponse>(request);
                var friends = _unitOfWork.Repository<MusicPassword>().GetAll().Include(x => x.TopicOfGame).Include(x => x.TopicOfGame.Topic)
                    .Select(x => new MusicPasswordResponse
                    {
                        Id = x.Id,
                        TopicOfGameId = x.TopicOfGameId,
                        Password=x.Password,
                        TopicName = x.TopicOfGame.Topic.Name,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<MusicPasswordResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<MusicPasswordResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<MusicPasswordResponse> UpdateMusicPasswordResource(int id, MusicPasswordRequest request)
        {
            try
            {
                MusicPassword musicPassword = _unitOfWork.Repository<MusicPassword>()
                      .Find(c => c.Id == id);

                if (musicPassword == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found musicPassword resource with id{id.ToString()}", "");

                var s = _unitOfWork.Repository<MusicPassword>().Find(s => s.Password == request.Password && s.Id != id);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var topic = _unitOfWork.Repository<Topic>().Find(x => x.Id == musicPassword.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic {request.TopicOfGameId} is not found !!!", "");
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordVersion", version += 1, expiryTime);
                _mapper.Map<MusicPasswordRequest, MusicPassword>(request, musicPassword);
                await _unitOfWork.Repository<MusicPassword>().Update(musicPassword, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<MusicPasswordResponse>(musicPassword);
                rs.TopicName = topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update music password resource error!!!!!", ex.Message);
            }
        }
    }
}
