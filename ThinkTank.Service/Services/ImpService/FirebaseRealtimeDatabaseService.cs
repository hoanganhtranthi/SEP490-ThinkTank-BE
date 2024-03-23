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
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "JVXT4lY97hJW3FR4HQ3U7FU3VQSr0sfGibYJkYXZ",
            BasePath = "https://thinktank-79ead-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;
        public FirebaseRealtimeDatabaseService()
        {
            client = new FirebaseClient(config);
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
        public async Task<bool> RemoveData<T>(string key)
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
    }
}
