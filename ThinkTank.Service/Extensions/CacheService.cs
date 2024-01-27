using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Repository.Extensions
{
   
    public class CacheService : ICacheService
    {
        private IDistributedCache _distributedCache;
        private IConfiguration configuration;
        private IDatabase redis;
        public CacheService(IDistributedCache distributed,IConfiguration configuration)
        {
            _distributedCache = distributed;
            this.configuration = configuration;
            redis= ConnectionMultiplexer.Connect(configuration["ConnectionStrings:RedisConnectionString"]).GetDatabase();
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
    }
}
