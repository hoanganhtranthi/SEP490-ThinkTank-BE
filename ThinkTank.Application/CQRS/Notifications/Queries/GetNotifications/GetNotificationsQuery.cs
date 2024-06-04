
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQuery:IGetTsQuery<PagedResults<NotificationResponse>>
    {
        public NotificationRequest NotificationRequest { get; }
        public GetNotificationsQuery(PagingRequest pagingRequest,NotificationRequest notificationRequest) : base(pagingRequest)
        {
            NotificationRequest = notificationRequest;
        }
        
    }
}
