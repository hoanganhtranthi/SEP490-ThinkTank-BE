using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ThinkTank.API.AppStart
{
    public static class HangfireConfig
    {
        public static void ConfigureHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage()
                    .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection"),
                        new SqlServerStorageOptions()
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true,
                            
                        });
                
            });
            services.AddHangfireServer();
        }
    }
}