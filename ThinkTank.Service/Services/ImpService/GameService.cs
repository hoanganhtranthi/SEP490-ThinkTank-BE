using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class GameService : IGameService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GameService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }     

        public async Task<GameResponse> GetGameById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Game Invalid", "");
                }
                var response = await _unitOfWork.Repository<Game>().GetAsync(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id {id.ToString()}", "");
                }

                var amount = _unitOfWork.Repository<Achievement>().GetAll().Where(x => x.GameId == id).Select(a=>a.AccountId).Distinct().Count();
                var rs= _mapper.Map<GameResponse>(response);
                rs.AmoutPlayer = amount;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Game By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<ReportGameLevelResponse>> GetGameLevelById(int id, PagingRequest paging)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Game Invalid", "");
                }

                var game = await _unitOfWork.Repository<Game>().GetAsync(u => u.Id == id);

                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id {id}", "");
                }

                var responseLevels = new List<ReportGameLevelResponse>();

                var achievements = _unitOfWork.Repository<Achievement>()
                    .GetAll()
                    .Where(x => x.GameId == id && x.Mark > 0)
                    .ToList();

                foreach (var achievement in achievements)
                {
                    var existingResponseLevel = responseLevels.FirstOrDefault(a => a.Level == achievement.Level);
                    if (existingResponseLevel == null)
                    {
                        var responseLevel = new ReportGameLevelResponse
                        {
                            Level = achievement.Level,
                            AmoutPlayer = 1,
                            GameMode = GetGameMode(achievement),
                            AccountIdsChecked=new List<int>()
                            
                        };
                        responseLevel.AccountIdsChecked.Add(achievement.AccountId);
                        responseLevels.Add(responseLevel);
                    }
                    else
                    {
                        if (!existingResponseLevel.AccountIdsChecked.Contains(achievement.AccountId))
                        {
                            existingResponseLevel.AmoutPlayer++;
                            existingResponseLevel.AccountIdsChecked.Add(achievement.AccountId);                          
                        }
                        if (existingResponseLevel.GameMode != GetGameMode(achievement))
                            existingResponseLevel.GameMode += ", " + GetGameMode(achievement);
                    }
                }
                var sort = PageHelper<ReportGameLevelResponse>.Sorting(paging.SortType, responseLevels, "Level");
                var result = PageHelper<ReportGameLevelResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Game By ID Error", ex.InnerException?.Message);
            }
        }

        private string GetGameMode(Achievement achievement)
        {
            var isSingleMode = _unitOfWork.Repository<AccountIn1vs1>()
                .GetAll()
                .Any(a =>
                    (a.AccountId1 == achievement.AccountId || a.AccountId2 == achievement.AccountId) && 
                    a.EndTime == achievement.CompletedTime);

            return isSingleMode ? "Multiplayer Mode" : "Single Mode";
        }

        public async Task<PagedResults<GameResponse>> GetGames(GameRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<GameResponse>(request);
                var games = _unitOfWork.Repository<Game>().GetAll().Select(x=>new GameResponse
                {
                    Id = x.Id,
                    Name=x.Name,
                    AmoutPlayer =_unitOfWork.Repository<Achievement>().GetAll().Where(x => x.GameId ==x.Id).Select(a => a.AccountId).Distinct().Count()
            })  .DynamicFilter(filter) .ToList();
                var sort = PageHelper<GameResponse>.Sorting(paging.SortType, games, paging.ColName);
                var result = PageHelper<GameResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get game list error!!!!!", ex.Message);
            }
        }
      
        }

    }
