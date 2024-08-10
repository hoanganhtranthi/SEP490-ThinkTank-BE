
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;

namespace ThinkTank.Application.Services.IService
{
    public interface ISlackService
    {
        SlackRequest CreateMessage(Exception exception, string name);

        Task SendMessage(SlackRequest slackRequest);
    }
}
