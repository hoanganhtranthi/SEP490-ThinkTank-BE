﻿using RedLockNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Response;

namespace Repository.Extensions
{
    public interface ICacheService
    {
        T GetData<T>(string key);
        void SetData<T>(string key, T value, DateTimeOffset expirationTime);
        object RemoveData(string key);
        //Task Finish(string key, bool success = true);   
       // void AddJob(RedisValue job);
        Task AddJobAsync<T>(T value, string key);
        Task<List<string>> GetJobsAsync(string key);
        Task<bool> DeleteJobAsync<T>(string key, T value);
        IRedLock AcquireLock(string key);
        // Task AddJobAsync(Dictionary<RedisValue, RedisValue> parametersDictionary);

    }
}
