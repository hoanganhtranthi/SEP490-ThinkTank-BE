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
        private IConfiguration _configuration;
        private IDatabase redis;
        private ConnectionMultiplexer connection;
        private readonly RedLockFactory _redLockFactory;

        public CacheService(IConfiguration configuration,RedLockFactory redLockFactory)
        {
            _configuration = configuration;
            _redLockFactory = redLockFactory;
            connection = ConnectionMultiplexer.Connect(_configuration["ConnectionStrings:RedisConnectionString"]);
            redis = ConnectionMultiplexer.Connect(_configuration["ConnectionStrings:RedisConnectionString"]).GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value=redis.StringGet(key);
              if (!string.IsNullOrEmpty(value.ToString()))
                  return JsonSerializer.Deserialize<T>(value);
              return default;
        }

        public object RemoveData(string key)
        {
            var _exist = redis.StringGet(key);
            Console.WriteLine(_exist.ToString());
            if (!string.IsNullOrEmpty(_exist.ToString()))
                return redis.KeyDelete(key);
            return false;
        }

        public void SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expirty = expirationTime.DateTime.Subtract(DateTime.Now);
            redis.StringSet(key, JsonSerializer.Serialize(value), expirty);
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
           // await db.HashSetAsync(key, parametersDictionary.Select(entries => new HashEntry(entries.Key, entries.Value)).ToArray());

            await db.ListLeftPushAsync(key, JsonSerializer.Serialize(value));
            await connection.GetSubscriber().PublishAsync("account1vs1", "");
        }
        public async Task<bool> DeleteJobAsync<T>(string key, T value)
        {
            string jsonValue = JsonSerializer.Serialize(value);

            long removedItemCount = await redis.ListRemoveAsync(key, jsonValue);

            return removedItemCount > 0;
        }
        public IRedLock AcquireLock(string key)
        {
            var redLock = _redLockFactory.CreateLock(key,
                expiryTime: TimeSpan.FromMilliseconds(10000),
                waitTime: TimeSpan.FromMilliseconds(250),
                retryTime: TimeSpan.FromMilliseconds(250));

            return redLock;
        }

    }
}
