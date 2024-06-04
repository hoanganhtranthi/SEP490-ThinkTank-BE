using FirebaseAdmin.Messaging;

namespace ThinkTank.Application.Services.IService
{
    public interface IFirebaseMessagingService
    {
        void SendToDevices(List<string> tokens, Notification notification, Dictionary<string, string> data);
        Task<bool> ValidToken(string fcmToken);
    }
}
