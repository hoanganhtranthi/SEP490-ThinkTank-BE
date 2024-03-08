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
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TopicService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<TopicResponse> CreateTopic(TopicRequest request)
        {
            try
            {

                var topic = _mapper.Map<TopicRequest, Topic>(request);
                var s = _unitOfWork.Repository<Topic>().Find(s => s.Name == request.Name && s.GameId==request.GameId);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {request.Name} has already !!!", "");
                }
                var g = _unitOfWork.Repository<Game>().Find(x => x.Id == request.GameId);
                if (g == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $" Game Id {request.GameId} is not found !!!", "");
               
                await _unitOfWork.Repository<Topic>().CreateAsync(topic);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<TopicResponse>(topic);
                rs.GameName = g.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Topic Error!!!", ex?.Message);
            }
        }

        public async Task<TopicResponse> GetTopicById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Topic Invalid", "");
                }
                var response = _unitOfWork.Repository<Topic>().GetAll()
                    .Include(c => c.Game).Where(a => a.Id == id).Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                       GameId=a.GameId,
                       GameName=a.Game.Name
                    }).SingleOrDefault();

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found topic with id {id.ToString()}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Topic By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<TopicResponse>> GetTopics(TopicRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<TopicResponse>(request);
                var friends = _unitOfWork.Repository<Topic>().GetAll().Include(a => a.Game)
                    .Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        GameId=a.GameId,
                        GameName=a.Game.Name
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<TopicResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<TopicResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get topic list error!!!!!", ex.Message);
            }
        }

        public async Task<TopicResponse> UpdateTopic(int id, TopicRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Topic Invalid", "");
                }
                Topic topic = _unitOfWork.Repository<Topic>()
                      .Find(c => c.Id == id);
                _mapper.Map<TopicRequest, Topic>(request,topic);
                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found topic with id{id.ToString()}", "");

                var s = _unitOfWork.Repository<Topic>().Find(s => s.Name == request.Name && s.Id != id && s.GameId==request.GameId);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {request.Name} has already !!!", "");
                }
                var g = _unitOfWork.Repository<Game>().Find(x => x.Id == request.GameId);
                if (g == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $" Game Id {request.GameId} is not found !!!", "");
                var rs=_mapper.Map<TopicResponse>(topic);
                await _unitOfWork.Repository<Topic>().Update(topic, id);
                await _unitOfWork.CommitAsync();
                rs.GameName = g.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update topic error!!!!!", ex.Message);
            }
        }
    }
}
