using RedLockNet;
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
        Task AddJobAsync<T>(T value, string key);
        Task<List<string>> GetJobsAsync(string key);
        Task<bool> DeleteJobAsync<T>(string key, T value);

    }
}
