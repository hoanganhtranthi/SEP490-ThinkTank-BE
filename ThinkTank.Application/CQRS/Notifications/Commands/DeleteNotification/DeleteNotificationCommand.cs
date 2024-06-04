

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Notifications.Commands.DeleteNotification
{
    public class DeleteNotificationCommand:ICommand<List<NotificationResponse>>
    {
        public List<int> Ids { get; }
        public DeleteNotificationCommand(List<int> id)
        {
            Ids = id;
        }
    }
}
