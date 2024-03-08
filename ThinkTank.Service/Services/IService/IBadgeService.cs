using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IBadgeService
    {
        Task<PagedResults<BadgeResponse>> GetBadges(BadgeRequest request, PagingRequest paging);
        Task<PagedResults<BadgeResponse>> GetBadgesIsCompleted(BadgeRequest request, PagingRequest paging);

    }
}
