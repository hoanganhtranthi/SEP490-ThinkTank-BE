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
    public class TopicService : ITopicService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TopicService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TopicResponse> CreateTopic(CreateTopicOfGameRequest request)
        {
            try
            {

                var topic = _mapper.Map<CreateTopicOfGameRequest, Topic>(request);
                var s = _unitOfWork.Repository<Topic>().Find(s => s.Name == request.Name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {request.Name} has already !!!", "");
                }

                List<TopicOfGameResponse> response = new List<TopicOfGameResponse>();
                List<TopicOfGame> topics = new List<TopicOfGame>();
                foreach (var game in request.GamesId)
                {
                    var g = _unitOfWork.Repository<Game>().Find(x => x.Id == game);
                    if (g == null)
                        throw new CrudException(HttpStatusCode.BadRequest, $" Game Id {game} is not found !!!", "");
                    TopicOfGame topicOfGame = new TopicOfGame();
                    TopicOfGameResponse topicOfGameResponse = new TopicOfGameResponse();
                    topicOfGame.GameId = game;
                    topicOfGame.Game = g;
                    await _unitOfWork.Repository<TopicOfGame>().CreateAsync(topicOfGame);
                    topicOfGameResponse = _mapper.Map<TopicOfGameResponse>(topicOfGame);
                    topicOfGameResponse.GameName = g.Name;
                    response.Add(topicOfGameResponse);
                    topics.Add(topicOfGame);
                }
                topic.TopicOfGames = topics;
                await _unitOfWork.Repository<Topic>().CreateAsync(topic);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<TopicResponse>(topic);
                rs.TopicOfGames = response;
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

        public async Task<TopicResponse> UpdateTopic(int id, string name)
        {
            try
            {
                Topic topic = _unitOfWork.Repository<Topic>()
                      .Find(c => c.Id == id);

                if (topic == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found topic with id{id.ToString()}", "");

                var s = _unitOfWork.Repository<Topic>().Find(s => s.Name == name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $" Topic Name {name} has already !!!", "");
                }               
                topic.Name= name;                
                await _unitOfWork.Repository<Topic>().Update(topic, id);
                await _unitOfWork.CommitAsync();
                return  _unitOfWork.Repository<Topic>().GetAll()
                    .Include(c => c.TopicOfGames).Where(a => a.Id == id).Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        TopicOfGames = new List<TopicOfGameResponse>
                    (a.TopicOfGames.Select(x => new TopicOfGameResponse
                    {
                        GameId = x.GameId,
                        Id = x.Id,
                        GameName = x.Game.Name
                    }))
                    }).SingleOrDefault();               
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

        public async Task<TopicResponse> GetTopicById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Topic Invalid", "");
                }
                var response = _unitOfWork.Repository<Topic>().GetAll()
                    .Include(c=>c.TopicOfGames).Where(a => a.Id == id).Select(a => new TopicResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    TopicOfGames=new List<TopicOfGameResponse>
                    (a.TopicOfGames.Select(x => new TopicOfGameResponse
                    {
                        GameId = x.GameId,
                        Id = x.Id,
                        GameName=x.Game.Name
                    }))
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
                var topics = _unitOfWork.Repository<Topic>().GetAll().Include(a => a.TopicOfGames)
                    .Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        TopicOfGames = new List<TopicOfGameResponse>
                    (a.TopicOfGames.Select(x => new TopicOfGameResponse
                    {
                        GameId = x.GameId,
                        Id = x.Id,
                        GameName = x.Game.Name
                    }))
                    }).DynamicFilter(filter).ToList();
                if(request.GameId != null)
                {
                    List<TopicResponse> topicResponse = new List<TopicResponse>();
                    foreach (var topic in topics)
                    {
                        if (topic.TopicOfGames != null)
                        {
                            var t = topic.TopicOfGames.SingleOrDefault(x => x.GameId == request.GameId);
                            if (t != null)
                                topicResponse.Add(topic);
                        }
                    }
                    topics = topicResponse;
                }
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
