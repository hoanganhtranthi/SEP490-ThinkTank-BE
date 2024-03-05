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
    public class FlipCardAndImagesWalkthroughResourceService : IFlipCardAndImagesWalkthroughResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public FlipCardAndImagesWalkthroughResourceService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async  Task<FlipCardAndImagesWalkthroughResponse> CreateFlipCardAndImagesWalkthroughResource(FlipCardAndImagesWalkthroughRequest createFlipCardAndImagesWalkthroughRequest)
        {
            try
            {
                var flipCardAndImagesWalkthrough = _mapper.Map<FlipCardAndImagesWalkthroughRequest, FlipCardAndImagesWalkthrough>(createFlipCardAndImagesWalkthroughRequest);
               
                var topic = _unitOfWork.Repository<TopicOfGame>().GetAll().Include(x=>x.Topic).SingleOrDefault(x => x.Id == flipCardAndImagesWalkthrough.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic of game {createFlipCardAndImagesWalkthroughRequest.TopicOfGameId} is not found !!!", "");

                await _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().CreateAsync(flipCardAndImagesWalkthrough);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughResponse>(flipCardAndImagesWalkthrough);
                rs.TopicName = topic.Topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add FlipCard And ImagesWalkthrough  Resource Error!!!", ex?.Message);
            }
        }

        public async Task<FlipCardAndImagesWalkthroughResponse> DeleteFlipCardAndImagesWalkthroughResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().GetAll().Include(x=>x.TopicOfGame.Topic).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughVersion", version += 1, expiryTime);

                var rs = _mapper.Map<FlipCardAndImagesWalkthroughResponse>(response);
                rs.TopicName = response.TopicOfGame.Topic.Name;
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

        public async Task<FlipCardAndImagesWalkthroughResponse> GetFlipCardAndImagesWalkthroughResourceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().GetAll().Include(x=>x.TopicOfGame).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughResponse>(response);
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

        public async Task<PagedResults<FlipCardAndImagesWalkthroughResponse>> GetFlipCardAndImagesWalkthroughResources(ResourceRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<FlipCardAndImagesWalkthroughResponse>(request);
                var friends = _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().GetAll().Include(x => x.TopicOfGame).Include(x => x.TopicOfGame.Topic)
                    .Select(x => new FlipCardAndImagesWalkthroughResponse
                    {
                        Id = x.Id,
                        TopicOfGameId = x.TopicOfGameId,
                        LinkImg=x.LinkImg,
                        TopicName = x.TopicOfGame.Topic.Name,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<FlipCardAndImagesWalkthroughResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<FlipCardAndImagesWalkthroughResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<FlipCardAndImagesWalkthroughResponse> UpdateFlipCardAndImagesWalkthroughResource(int id, FlipCardAndImagesWalkthroughRequest request)
        {
            try
            {
                FlipCardAndImagesWalkthrough flipCardAndImagesWalkthrough = _unitOfWork.Repository<FlipCardAndImagesWalkthrough>()
                      .Find(c => c.Id == id);

                if (flipCardAndImagesWalkthrough == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found flipCardAndImagesWalkthrough resource with id{id.ToString()}", "");

                var topic = _unitOfWork.Repository<TopicOfGame>().GetAll().Include(x => x.Topic).SingleOrDefault(x => x.Id == flipCardAndImagesWalkthrough.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic of game {request.TopicOfGameId} is not found !!!", "");
                
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughVersion", version += 1, expiryTime);
                
                _mapper.Map<FlipCardAndImagesWalkthroughRequest, FlipCardAndImagesWalkthrough>(request, flipCardAndImagesWalkthrough);
                await _unitOfWork.Repository<FlipCardAndImagesWalkthrough>().Update(flipCardAndImagesWalkthrough, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughResponse>(flipCardAndImagesWalkthrough);
                rs.TopicName = topic.Topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update FlipCard And ImagesWalkthrough resource error!!!!!", ex.Message);
            }
        }
    }
}
