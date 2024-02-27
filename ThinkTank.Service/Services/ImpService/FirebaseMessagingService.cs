using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class FirebaseMessagingService : IFirebaseMessagingService
    {
        private readonly static FirebaseMessaging _fm = FirebaseMessaging.DefaultInstance;
        public async  Task SendNotificationAsync(string FcmToken, Notification notification)
        {
            var googleCredential = (ServiceAccountCredential)GoogleCredential.GetApplicationDefaultAsync().Result.CreateScoped("https://www.googleapis.com/auth/firebase.messaging").UnderlyingCredential;
            try
            {
                var data = new
                {
                    message = new
                    {
                        token = FcmToken,
                        notification = notification
                    }
                };
                var jsonBody = JsonConvert.SerializeObject(data);
                using (var client = new HttpClient())
                {
                    var requestUri = $"https://fcm.googleapis.com/v1/projects/thinktank-ad0b3/messages:send";
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await googleCredential.GetAccessTokenForRequestAsync());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(requestUri, content);

                    Console.WriteLine($"{await response.Content.ReadAsStringAsync()} messages were sent successfully"); 
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
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

        public async void SendToTopic(string topic, Notification notification, Dictionary<string, string> data)
        {
            // See documentation on defining a message payload.
            var message = new Message()
            {
                Data = data,
                Notification = notification,
                Topic = topic,
            };

            // Send a message to the devices subscribed to the provided topic.
            var response = await _fm.SendAsync(message);
            Console.WriteLine($"Successfully send message to topic '{topic}': {response}");
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
