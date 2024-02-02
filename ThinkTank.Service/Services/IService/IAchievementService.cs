using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAchievementService
    {
        Task<PagedResults<AchievementResponse>> GetAchievements(AchievementRequest request, PagingRequest paging);
        Task<AchievementResponse> CreateAchievement(CreateAchievementRequest createAchievementRequest);
        Task<AchievementResponse> GetAchievementById(int id);
    }
}
