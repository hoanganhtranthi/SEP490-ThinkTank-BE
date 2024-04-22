using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Service.Services.IService;
using static Google.Apis.Requests.BatchRequest;

namespace ThinkTank.Service.Services.ImpService
{
    public class FirebaseRealtimeDatabaseService:IFirebaseRealtimeDatabaseService
    {
        private readonly IFirebaseClient client;
        private readonly IFirebaseClient clientOfFlutterRealtimeDatabase;
        private readonly IConfiguration configuration;
        public FirebaseRealtimeDatabaseService(IConfiguration configuration)
        {
            this.configuration = configuration;
            client = new FirebaseClient(new FirebaseConfig
            {
                AuthSecret = configuration["Firebase:AuthSecret"],
                BasePath = configuration["Firebase:BasePath"]
            });

            clientOfFlutterRealtimeDatabase = new FirebaseClient(new FirebaseConfig
            {
                BasePath = configuration["Firebase:BasePathOfFlutterRealtimeDatabase"]
            });
        }
        public async Task SetAsync<T>(string key,T value)
        {
           await client.SetAsync<T>(key,value);
        }
        public async Task<T> GetAsync<T>(string key)
        {
            FirebaseResponse response = await client.GetAsync(key);
            if (response.Body != "null")
            {
                return response.ResultAs<T>();
            }
            else
            {
                return default(T);
            }
        }
        public async Task<T> GetAsyncOfFlutterRealtimeDatabase<T>(string key)
        {
            FirebaseResponse response = await clientOfFlutterRealtimeDatabase.GetAsync(key);
            if (response.Body != "null")
            {
                return response.ResultAs<T>();
            }
            else
            {
                return default(T);
            }
        }
        public async Task<bool> RemoveData(string key)
        {
            var _exist = await client.GetAsync(key);
            Console.WriteLine(_exist.ToString());
            if (_exist.Body != "null")
            {
                 await client.DeleteAsync(key);
                return true ;
            }
            else
            {
                return false;
            }
        }
        public async Task SetAsyncOfFlutterRealtimeDatabase<T>(string key, T value)
        {
            await clientOfFlutterRealtimeDatabase.SetAsync<T>(key, value);
        }
    }
}
