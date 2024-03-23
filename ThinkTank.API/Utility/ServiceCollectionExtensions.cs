using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace ThinkTank.API.Utility
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisConnection(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddSingleton(provider =>
            {
                return ConnectionMultiplexer.Connect(configuration["ConnectionStrings:RedisConnectionString"]);
            });
        }

        public static IServiceCollection AddRedLock(this IServiceCollection services)
        {
            return services.AddSingleton(provider =>
            {
                var multiplexer = provider.GetRequiredService<ConnectionMultiplexer>();
                var redLockMultiplexers = new List<RedLockMultiplexer>()
                {
                    multiplexer
                };
                return RedLockFactory.Create(redLockMultiplexers);
            });
        }
    }
}
