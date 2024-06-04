
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Topics.Queries.GetTopicById
{
    public class GetTopicByIdQuery : IGetTByIdQuery<TopicResponse>
    {
        public GetTopicByIdQuery(int id) : base(id)
        {
        }
    }
}
