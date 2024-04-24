

namespace Repository.Extensions
{
    public interface ICacheService
    {
        Task AddJobAsync<T>(T value, string key);
        Task<List<string>> GetJobsAsync(string key);
        Task<bool> DeleteJobAsync<T>(string key, T value);

    }
}
