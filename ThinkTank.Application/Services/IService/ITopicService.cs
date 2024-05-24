
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface ITopicService
    {
        Task<PagedResults<TopicResponse>> GetTopics(TopicRequest request, PagingRequest paging);
        Task<TopicResponse> GetTopicById(int id);
        Task<TopicResponse> CreateTopic(CreateTopicRequest request);
    }
}
