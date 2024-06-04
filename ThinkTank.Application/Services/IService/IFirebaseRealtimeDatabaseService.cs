namespace ThinkTank.Application.Services.IService
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
