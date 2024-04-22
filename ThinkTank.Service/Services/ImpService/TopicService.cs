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
        public async Task<TopicResponse> CreateTopic(CreateTopicRequest request)
        {
            try
            {
                if (request.GameId <= 0 || request.Name == null || request.Name == "")
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                var topic = _mapper.Map<CreateTopicRequest, Topic>(request);

                var existingTopic = _unitOfWork.Repository<Topic>().Find(s => s.Name == request.Name && s.GameId==request.GameId);
                if (existingTopic != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {request.Name} has already !!!", "");
                }
                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == request.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $" Game Id {request.GameId} is not found !!!", "");
               
                await _unitOfWork.Repository<Topic>().CreateAsync(topic);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<TopicResponse>(topic);
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
                var response = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking()
                    .Include(c => c.Game).Where(a => a.Id == id).Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        GameId=a.GameId,
                        Assets=new List<AssetResponse>(a.Assets.Select(x=>new AssetResponse
                        {
                            Id=x.Id,
                            GameId=x.Topic.GameId,
                            GameName=x.Topic.Game.Name,
                            Status=x.Status,
                            TopicId=a.Id,
                            TopicName=a.Name,
                            Value=x.Value,
                            Version=x.Version,
                            Answer= x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath) : null
                        }))
                    }).SingleOrDefault();

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found topic with id {id}", "");
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
                var topics = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking().Include(a => a.Game)
                    .Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        GameId=a.GameId,
                        Assets = new List<AssetResponse>(a.Assets.Select(x => new AssetResponse
                        {
                            Id = x.Id,
                            GameId = x.Topic.GameId,
                            GameName = x.Topic.Game.Name,
                            Status = x.Status,
                            TopicId = a.Id,
                            TopicName = a.Name,
                            Value = x.Value,
                            Version = x.Version,
                            Answer = x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath) : null
                        }))
                    }).DynamicFilter(filter).ToList();

                if (request.IsHavingAsset == Helpers.Enum.StatusTopicType.True)
                    topics = topics.Where(x => x.Assets.Count() > 0).ToList();

                if (request.IsHavingAsset == Helpers.Enum.StatusTopicType.False)
                    topics = topics.Where(x => x.Assets.Count() == 0).ToList();

                else topics = topics.ToList();

                var sort = PageHelper<TopicResponse>.Sorting(paging.SortType, topics, paging.ColName);
                var result = PageHelper<TopicResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get topic list error!!!!!", ex.Message);
            }
        }

       
    }
}
