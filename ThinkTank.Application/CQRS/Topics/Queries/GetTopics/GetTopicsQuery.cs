

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Topics.Queries.GetTopics
{
    public class GetTopicsQuery:IGetTsQuery<PagedResults<TopicResponse>>
    {
        public TopicRequest TopicRequest { get; }
        public GetTopicsQuery(PagingRequest pagingRequest, TopicRequest topicRequest) : base(pagingRequest)
        {
            TopicRequest = topicRequest;
        }

    }
}
