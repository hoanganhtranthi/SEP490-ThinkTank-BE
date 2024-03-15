using AutoMapper;
using AutoMapper.QueryableExtensions;
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
                var response = _unitOfWork.Repository<Game>().GetAll().Include(x => x.Topics).Select(x => new GameResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    AmoutPlayer= _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Topic).Where(x => x.Topic.GameId == id).Select(a => a.AccountId).Distinct().Count(),
                    Topics = new List<TopicResponse>(x.Topics.Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name
                    }))
                }).SingleOrDefault(x=>x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id {id.ToString()}", "");
                }
                return response;
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

        public async Task<dynamic> GetReportOGame()
        {
            try
            {
                var currentDateMonth = DateTime.Now.Month;

                var totalSinglePlayer = _unitOfWork.Repository<Achievement>()
                    .GetAll()
                    .Where(x => x.CompletedTime.Month == currentDateMonth)
                    .Select(x => x.AccountId)
                    .Distinct()
                    .Count();

                var totalMultiplayerMode = _unitOfWork.Repository<AccountInRoom>()
                    .GetAll()
                    .Where(x => x.CompletedTime.Month == currentDateMonth)
                    .Select(x => x.AccountId)
                    .Distinct()
                    .Count();

                HashSet<int> uniqueAccounts = new HashSet<int>();

                var accountPairs = _unitOfWork.Repository<AccountIn1vs1>()
                    .GetAll()
                    .Select(x => new { x.AccountId1, x.AccountId2 });

                foreach (var pair in accountPairs)
                {
                    uniqueAccounts.Add(pair.AccountId1);
                    uniqueAccounts.Add(pair.AccountId2);
                }

                var total1vs1Mode = uniqueAccounts.Count;


                var totalContest = _unitOfWork.Repository<Contest>()
                    .GetAll()
                    .Where(x => x.StartTime.Month == currentDateMonth)
                    .Count();

                var totalRoom = _unitOfWork.Repository<AccountInRoom>()
                    .GetAll()
                    .Where(x => x.CompletedTime.Month == currentDateMonth)
                    .Select(x => x.AccountId)
                    .Distinct()
                    .Count();

                var totalUser = _unitOfWork.Repository<Account>()
                    .GetAll()
                    .Count();

                var totalNewbieUser = _unitOfWork.Repository<Account>()
                    .GetAll()
                    .Where(x => x.RegistrationDate.Value.Month == currentDateMonth)
                    .Count();

                var total = total1vs1Mode + totalMultiplayerMode + totalSinglePlayer;
                var percent1vs1Mode = (double)total1vs1Mode / total * 100;
                var percentMultiplayerMode = (double)totalMultiplayerMode / total * 100;
                var percentSinglePlayer = (double)totalSinglePlayer / total * 100;

                return new
                {
                    TotalSinglePlayerMode = percentSinglePlayer,
                    Total1vs1Mode = percent1vs1Mode,
                    TotalMultiplayerMode = percentMultiplayerMode,
                    TotalContest = totalContest,
                    TotalRoom = totalRoom,
                    TotalUser = totalUser,
                    TotalNewbieUser = totalNewbieUser,
                };

            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Report Game  Error", ex.InnerException?.Message);
            }
        }
        public async Task<PagedResults<GameResponse>> GetGames(GameRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<GameResponse>(request);
                var games = _unitOfWork.Repository<Game>().GetAll().Include(x=>x.Topics).Select(x=>new GameResponse
                {
                    Id = x.Id,
                    Name=x.Name,
                    Topics=new List<TopicResponse>(x.Topics.Select(a=>new TopicResponse
                    {
                        Id=a.Id,
                        Name=a.Name
                    }))
            })  .DynamicFilter(filter) .ToList();
                foreach(var game in games)
                {
                    game.AmoutPlayer = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Topic).Where(x => x.Topic.GameId == game.Id).Select(a => a.AccountId).Distinct().Count();
                  
                }
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
