
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;
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
        }
        public async Task<bool> DeleteJobAsync<T>(string key, T value)
        {
            string jsonValue = JsonSerializer.Serialize(value);

            long removedItemCount = await redis.ListRemoveAsync(key, jsonValue);

            return removedItemCount > 0;
        }

    }
}
