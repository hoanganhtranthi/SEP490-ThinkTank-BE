using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Services.IService
{
    public interface IFirebaseRealtimeDatabaseService
    {
        Task SetAsync<T>(string key, T value);
        Task<T> GetAsync<T>(string key);
        Task<bool> RemoveData(string key);

        Task<T> GetAsyncOfFlutterRealtimeDatabase<T>(string key);
        Task SetAsyncOfFlutterRealtimeDatabase<T>(string key, T value);
        Task<bool> RemoveDataFlutterRealtimeDatabase(string key);
    }
}
