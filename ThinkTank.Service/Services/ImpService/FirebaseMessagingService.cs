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
        public async Task<int>SendMessage(List<string> tokens, Notification notification, Dictionary<string, string> data)
        {
            var message = new MulticastMessage()
            {
                Tokens = tokens,
                Data = data,
                Notification = notification

            };

            var response = await _fm.SendMulticastAsync(message);
            return response.SuccessCount;
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
        }

        public async void Subcribe(IReadOnlyList<string> tokens, string topic)
        {
            var response = await _fm.SubscribeToTopicAsync(tokens, topic);
            Console.WriteLine($"Successfully subcribe users to topic '{topic}': {response.SuccessCount} sent");
        }

        public async void Unsubcribe(IReadOnlyList<string> tokens, string topic)
        {
            var response = await _fm.UnsubscribeFromTopicAsync(tokens, topic);
            Console.WriteLine($"Successfully unsubcribe users from topic '{topic}': {response.SuccessCount} sent");
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
