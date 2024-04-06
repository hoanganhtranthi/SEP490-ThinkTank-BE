using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RedLockNet;
using RedLockNet.SERedis;
using StackExchange.Redis;
using StackExchange.Redis.MultiplexerPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Response;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Repository.Extensions
{
   
    public class CacheService : ICacheService
    {
        private static CacheService _instance;
        private static readonly object SyncRoot = new object();
        private static readonly Lazy<CacheService> Lazy = new Lazy<CacheService>(() => new CacheService());
        private IDatabase redis;
        private IConnectionMultiplexer connection;

        public CacheService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var redisConnectionString = configuration.GetConnectionString("RedisConnectionString");
            connection = ConnectionMultiplexer.Connect(redisConnectionString);
            redis = connection.GetDatabase();
        }
        public static CacheService Instance
        {
            get
            {
                if (_instance == null)
                    lock (SyncRoot)
                    {
                        return Lazy.Value;
                    }

                return _instance;
            }
        }
        public async Task EnqueueJobAsync(int job)
        {
            await redis.ListLeftPushAsync("jobQueue", JsonSerializer.Serialize(job));
            await redis.HashSetAsync(job.ToString(), "status", "queued");
        }
        // Fetch all jobs in the queue, along with their status
        public async Task<List<string>> GetJobsAsync(string key)
        {
            var jobs = redis.ListRangeAsync(key).Result;
            var jobList = new List<string>();
            foreach (var job in jobs)
            {
                var redisJob = JsonSerializer.Deserialize<string>(job);
                jobList.Add(redisJob);
            }
            return jobList;
        }
        public async Task AddJobAsync<T>(T value, string key)
        {

            var db = redis;
            await db.ListLeftPushAsync(key, JsonSerializer.Serialize(value));
            await connection.GetSubscriber().PublishAsync("account1vs1", "");
        }
        public async Task<bool> DeleteJobAsync<T>(string key, T value)
        {
            string jsonValue = JsonSerializer.Serialize(value);

            long removedItemCount = await redis.ListRemoveAsync(key, jsonValue);

            return removedItemCount > 0;
        }

    }
}
