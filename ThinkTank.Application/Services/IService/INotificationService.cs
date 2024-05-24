
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface INotificationService
    {
        Task<PagedResults<NotificationResponse>> GetNotifications(NotificationRequest request, PagingRequest paging);
        Task<NotificationResponse> GetNotificationById(int id);
        Task<NotificationResponse> GetToUpdateStatus(int id);
        Task<List<NotificationResponse>> DeleteNotification(List<int> id);
    }
}
