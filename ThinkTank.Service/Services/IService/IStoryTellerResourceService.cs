using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IStoryTellerResourceService
    {
        Task<PagedResults<StoryTellerResponse>> GetStoryTellerResources(ResourceRequest request, PagingRequest paging);
        Task<StoryTellerResponse> CreateStoryTellerResource(StoryTellerRequest createStoryTellerRequest);
        Task<StoryTellerResponse> GetStoryTellerResourceById(int id);
        Task<StoryTellerResponse> UpdateStoryTellerResource(int id, StoryTellerRequest request);
        Task<StoryTellerResponse> DeleteStoryTellerResource(int id);
    }
}
