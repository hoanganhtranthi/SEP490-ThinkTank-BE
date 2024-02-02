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
    public class StoryTellerResourceService : IStoryTellerResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public StoryTellerResourceService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<StoryTellerResponse> CreateStoryTellerResource(StoryTellerRequest createStoryTellerRequest)
        {
            try
            {
                var storyTeller = _mapper.Map<StoryTellerRequest, StoryTeller>(createStoryTellerRequest);
                var s = _unitOfWork.Repository<StoryTeller>().Find(s => s.Description == createStoryTellerRequest.Description);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var topic = _unitOfWork.Repository<Topic>().Find(x => x.Id == storyTeller.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic {createStoryTellerRequest.TopicOfGameId} is not found !!!", "");

                List<AnswerOfStoryTeller> answerOfStoryTellers = new List<AnswerOfStoryTeller>();
                foreach(var item in createStoryTellerRequest.AnswerOfStoryTellers)
                {
                    if(item.OrdinalNumber <=0 || item.OrdinalNumber > createStoryTellerRequest.AnswerOfStoryTellers.Count)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid ordinal number !!!", "");
                    if (createStoryTellerRequest.AnswerOfStoryTellers.SingleOrDefault(x=>x.OrdinalNumber==item.OrdinalNumber && x.LinkImg != item.LinkImg)!= null)
                        throw new CrudException(HttpStatusCode.BadRequest, "This ordinal number has already !!!", "");
                    var answerOfStoryTeller= _mapper.Map<AnswerOfStoryTellerRequest, AnswerOfStoryTeller>(item);
                    answerOfStoryTellers.Add(answerOfStoryTeller);                   
                }

                storyTeller.AnswerOfStoryTellers = answerOfStoryTellers;
                await _unitOfWork.Repository<StoryTeller>().CreateAsync(storyTeller);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("StoryTellerVersion");
                if (version != null)
                    _cacheService.SetData<int>("StoryTellerVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<StoryTellerResponse>(storyTeller);
                rs.AnswerOfStoryTellers = _mapper.Map<List<AnswerOfStoryTellerResponse>>(answerOfStoryTellers);
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

        public async Task<StoryTellerResponse> DeleteStoryTellerResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<StoryTeller>().GetAll().Include(x => x.AnswerOfStoryTellers).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<AnswerOfStoryTeller>().DeleteRange(response.AnswerOfStoryTellers.ToArray());
                _unitOfWork.Repository<StoryTeller>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("StoryTellerVersion");
                if (version != null)
                    _cacheService.SetData<int>("StoryTellerVersion", version += 1, expiryTime);

                var rs = _mapper.Map<StoryTellerResponse>(response);
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

        public async Task<StoryTellerResponse> GetStoryTellerResourceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<StoryTeller>().GetAll().Include(x => x.AnswerOfStoryTellers).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<StoryTellerResponse>(response);
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

        public async Task<PagedResults<StoryTellerResponse>> GetStoryTellerResources(ResourceRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<StoryTellerResponse>(request);
                var friends = _unitOfWork.Repository<StoryTeller>().GetAll().Include(x=>x.AnswerOfStoryTellers).Include(x => x.TopicOfGame).Include(x => x.TopicOfGame.Topic)
                    .Select(x => new StoryTellerResponse
                    {
                        Id = x.Id,
                        TopicOfGameId = x.TopicOfGameId,
                        Description=x.Description,
                        AnswerOfStoryTellers=_mapper.Map<List<AnswerOfStoryTellerResponse>>(x.AnswerOfStoryTellers),
                        TopicName = x.TopicOfGame.Topic.Name,
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<StoryTellerResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<StoryTellerResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<StoryTellerResponse> UpdateStoryTellerResource(int id, StoryTellerRequest request)
        {
            try
            {
                StoryTeller storyTeller = _unitOfWork.Repository<StoryTeller>()
                      .GetAll().Include(x=>x.AnswerOfStoryTellers).SingleOrDefault(x=>x.Id==id);

                if (storyTeller == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found storyTeller resource with id{id.ToString()}", "");

                var s = _unitOfWork.Repository<StoryTeller>().Find(s => s.Description == request.Description && s.Id != id);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var topic = _unitOfWork.Repository<Topic>().Find(x => x.Id == storyTeller.TopicOfGameId);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This topic {request.TopicOfGameId} is not found !!!", "");

                _unitOfWork.Repository<AnswerOfStoryTeller>().DeleteRange(storyTeller.AnswerOfStoryTellers.ToArray());

                List<AnswerOfStoryTeller> answerOfStoryTellers = new List<AnswerOfStoryTeller>();
                foreach (var item in request.AnswerOfStoryTellers)
                {
                    if (item.OrdinalNumber <= 0 || item.OrdinalNumber > request.AnswerOfStoryTellers.Count)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid ordinal number !!!", "");
                    if (request.AnswerOfStoryTellers.SingleOrDefault(x => x.OrdinalNumber == item.OrdinalNumber && x.LinkImg != item.LinkImg) != null)
                        throw new CrudException(HttpStatusCode.BadRequest, "This ordinal number has already !!!", "");
                    var answerOfStoryTeller = _mapper.Map<AnswerOfStoryTellerRequest, AnswerOfStoryTeller>(item);
                    answerOfStoryTellers.Add(answerOfStoryTeller);
                }

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("StoryTellerVersion");
                if (version != null)
                    _cacheService.SetData<int>("StoryTellerVersion", version += 1, expiryTime);

                _mapper.Map<StoryTellerRequest, StoryTeller>(request, storyTeller);
                storyTeller.AnswerOfStoryTellers = answerOfStoryTellers;
                await _unitOfWork.Repository<StoryTeller>().Update(storyTeller, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<StoryTellerResponse>(storyTeller);
                rs.TopicName = topic.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update story teller resource error!!!!!", ex.Message);
            }
        }
    }
}
