using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IGameService
    {
        Task<PagedResults<GameResponse>> GetGames(GameRequest request, PagingRequest paging);
        Task<GameResponse> GetGameById(int id);
        Task<PagedResults<ReportGameLevelResponse>> GetGameLevelById(int id,PagingRequest paging);
    }
}
