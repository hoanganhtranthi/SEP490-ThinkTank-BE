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
    public class AchievementService : IAchievementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AchievementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AchievementResponse> CreateAchievement(CreateAchievementRequest createAchievementRequest)
        {
            try
            {
                var achievement = _mapper.Map<CreateAchievementRequest, Achievement>(createAchievementRequest);
                var game = _unitOfWork.Repository<Game>().Find(x => x.Id == createAchievementRequest.GameId);
                if (game == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This game id {createAchievementRequest.GameId} is not found !!!", "");

                var account = _unitOfWork.Repository<Account>().Find(x => x.Id == createAchievementRequest.AccountId);
                if (account == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This account id {createAchievementRequest.AccountId} is not found !!!", "");

                var levels = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.AccountId == createAchievementRequest.AccountId && createAchievementRequest.GameId == x.GameId).OrderBy(x => x.Level).ToList();
                var level = 0;
                if (levels.Count() > 0)
                    level = levels.LastOrDefault().Level;
                else level = 0;
                if (createAchievementRequest.Level > level + 1 || createAchievementRequest.Level <= 0)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Level", "");
                achievement.CompletedTime = DateTime.Now;
                await _unitOfWork.Repository<Achievement>().CreateAsync(achievement);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AchievementResponse>(achievement);
                rs.Username = account.UserName;
                rs.GameName = game.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Achievement Error!!!", ex?.Message);
            }
        }

        public async Task<AchievementResponse> GetAchievementById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Achievement Invalid", "");
                }
                var response = _unitOfWork.Repository<Achievement>().GetAll().Include(c=>c.Account).Include(c=>c.Game).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found achievement with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AchievementResponse>(response);
                rs.Username=response.Account.UserName;
                rs.GameName=response.Game.Name;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Achievement By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AchievementResponse>> GetAchievements(AchievementRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<AchievementResponse>(request);
                var friends = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Account).Include(x => x.Game)
                    .Select(x => new AchievementResponse
                    {
                        Id = x.Id,
                        GameName = x.Game.Name,
                        AccountId=x.AccountId,
                        CompletedTime=x.CompletedTime,
                        Duration=x.Duration,
                        GameId=x.GameId,
                        Level=x.Level,
                        Mark=x.Mark,
                        Username=x.Account.UserName
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AchievementResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<AchievementResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get achievement list error!!!!!", ex.Message);
            }
        }
    }
}
