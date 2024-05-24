using FirebaseAdmin.Messaging;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.Services.ImpService
{
    public class FirebaseMessagingService : IFirebaseMessagingService
    {
        private readonly static FirebaseMessaging _fm = FirebaseMessaging.DefaultInstance;
        public async void SendToDevices(List<string> tokens, Notification notification, Dictionary<string, string> data)
        {
            var message = new MulticastMessage()
            {
                Tokens = tokens,
                Data = data,
                Notification = notification

            };

            var response = await _fm.SendMulticastAsync(message);
            Console.WriteLine($"{response.SuccessCount} messages were sent successfully");
        }
       
        public async Task<bool> ValidToken(string fcmToken)
        {
            if (fcmToken == null || fcmToken.Trim().Length == 0)
                return false;
            var result = await _fm.SendMulticastAsync(new MulticastMessage()
            {
                Tokens = new List<string>()
                {
                    fcmToken
                },

            }, true);

            return result.FailureCount == 0;

        }
    }
}
