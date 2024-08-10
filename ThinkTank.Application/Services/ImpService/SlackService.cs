
using FireSharp.Response;
using Hangfire.Server;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using StackExchange.Redis;
using System.Security.Cryptography.X509Certificates;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using static System.Net.Mime.MediaTypeNames;

namespace ThinkTank.Application.Services.ImpService
{
    public class SlackService : ISlackService
    {
        private readonly IConfiguration _configuration;
        private readonly string _postMessage;
        private readonly string _appToken;
        private readonly string _channelId;
        public SlackService(IConfiguration configuration)
        {
            _configuration = configuration;
            var section = _configuration.GetSection("Slack");
            _postMessage = section["PostMessage"];
            _appToken = section["AppToken"];
            _channelId = section["ChannelId"];
        }

        public SlackRequest CreateMessage(Exception exception, string name)
        {
            SlackRequest slackRequest = new SlackRequest
            {
                channel = $"{_channelId}",
                blocks = new List<object>
                {
                     new
                    {
                        type = "divider"
                    },
                    new
                    {
                        type = "section",
                        text = new
                        {
                            type = "mrkdwn",
                            text = $"Name: {name} \n"+
                         $"Message: {exception.Message} \n"+
                         $"StackTrace: {exception.StackTrace}"
                        },
                         
                    },
                    new
                    {
                        type = "divider"
                    }
                }
            };
            return slackRequest;
        }

        public async Task SendMessage(SlackRequest slackRequest)
        {
            var client = new RestClient(_postMessage);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", _appToken));
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(slackRequest), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            // throw exception if sending failed
            if (response.IsSuccessStatusCode == false)
            {
                throw new Exception(
                    "failed to send message. error: " + response.ErrorMessage
                );
            }
        }
    }
}
