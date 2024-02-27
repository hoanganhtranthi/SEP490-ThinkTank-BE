using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Services.IService
{
    public interface IFirebaseMessagingService
    {
        Task SendNotificationAsync(string FcmToken, Notification notification);
        void SendToDevices(List<string> tokens, Notification notification, Dictionary<string, string> data);
        Task<bool> ValidToken(string fcmToken);
        void SendToTopic(string topic, Notification notification, Dictionary<string, string> data);
    }
}
