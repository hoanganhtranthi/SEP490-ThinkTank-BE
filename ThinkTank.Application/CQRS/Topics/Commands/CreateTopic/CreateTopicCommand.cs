

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Topics.Commands.CreateTopic
{
    public class CreateTopicCommand:ICommand<TopicResponse>
    {
        public CreateTopicRequest CreateTopicRequest { get; }
        public CreateTopicCommand(CreateTopicRequest createTopicRequest)
        {
            CreateTopicRequest = createTopicRequest;
        }
    }
}
