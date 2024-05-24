
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IGameService
    {
        Task<PagedResults<GameResponse>> GetGames(GameRequest request, PagingRequest paging);
        Task<GameResponse> GetGameById(int id);
        Task<dynamic> GetReportOGame();
    }
}
