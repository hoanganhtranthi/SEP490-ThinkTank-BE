

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IAchievementService
    {
        Task<PagedResults<AchievementResponse>> GetAchievements(AchievementRequest request, PagingRequest paging);
        Task<AchievementResponse> CreateAchievement(CreateAchievementRequest createAchievementRequest);
        Task<AchievementResponse> GetAchievementById(int id);
        Task<PagedResults<LeaderboardResponse>> GetLeaderboard(int id, PagingRequest paging,int? accountId);
    }
}
