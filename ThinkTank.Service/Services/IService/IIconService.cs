using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IIconService
    {
        Task<PagedResults<IconResponse>> GetIcons(IconRequest request, PagingRequest paging);
        Task<IconResponse> GetIconById(int id);
        Task<IconResponse> GetToUpdateStatus(int id);
    }
}
