

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Notifications.Commands.UpdateStatusNotification
{
    public class UpdateStatusNotificationCommand:ICommand<NotificationResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public UpdateStatusNotificationCommand(int id)
        {
            Id = id;
        }
    }
}
