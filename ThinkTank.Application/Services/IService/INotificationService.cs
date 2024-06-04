namespace ThinkTank.Application.Services.IService
{
    public interface INotificationService
    {
        Task SendNotification(List<string> fcms, string message, string title, string imgUrl, int id);
        Task SendNotification(List<string> fcms, string message, string title, string imgUrl, List<int> ids);
    }
}
