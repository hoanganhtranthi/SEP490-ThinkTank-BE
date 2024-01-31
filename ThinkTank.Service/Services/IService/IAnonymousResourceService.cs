using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAnonymousResourceService
    {
        Task<PagedResults<AnonymousResponse>> GetAnonymousResources(ResourceRequest request, PagingRequest paging);
        Task<AnonymousResponse> CreateAnonymousResource(AnonymousRequest createAnonymousRequest);
        Task<AnonymousResponse> GetAnonymousResourceById(int id);
        Task<AnonymousResponse> UpdateAnonymousResource(int id, AnonymousRequest request);
        Task<AnonymousResponse> DeleteAnonymousResource(int id);
    }
}
