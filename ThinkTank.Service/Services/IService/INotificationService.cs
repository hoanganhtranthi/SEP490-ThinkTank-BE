using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface INotificationService
    {
        Task<PagedResults<NotificationResponse>> GetNotifications(NotificationRequest request, PagingRequest paging);
        Task<NotificationResponse> GetNotificationById(int id);
        Task<NotificationResponse> GetToUpdateStatus(int id);
        Task<List<NotificationResponse>> DeleteNotification(List<int> id);
    }
}
