

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IIconService
    {
        Task<PagedResults<IconResponse>> GetIcons(IconRequest request, PagingRequest paging);
        Task<IconResponse> GetIconById(int id);
    }
}
