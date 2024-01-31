using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface ITopicService
    {
        Task<PagedResults<TopicResponse>> GetTopics(TopicRequest request, PagingRequest paging);
        Task<TopicResponse> GetTopicById(int id);
        Task<TopicResponse> CreateTopic(CreateTopicOfGameRequest request);
        Task<TopicResponse> UpdateTopic(int id, string name);
    }
}
