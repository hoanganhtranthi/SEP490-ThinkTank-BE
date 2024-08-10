
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using ThinkTank.Application.Accounts.DomainServices.ImpServices;
using ThinkTank.Application.CQRS.Accounts.DomainServices.IService;
using ThinkTank.Application.Repository;
using ThinkTank.Application.Services.ImpService;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Infrastructures.DatabaseContext;
using ThinkTank.Infrastructures.Mapper;
using ThinkTank.Infrastructures.Repository;
using ThinkTank.Infrastructures.UnitOfWorkRepo;
using INotificationService = ThinkTank.Application.Services.IService.INotificationService;

namespace ThinkTank.Infrastructures
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, IConfiguration configuration)
        {
            #region DI_SERVICES      
            services.AddScoped<IHashPasswordService, HashPasswordService>();
            services.AddScoped<IBadgesService, BadgesService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFirebaseMessagingService, FirebaseMessagingService>();
            services.AddScoped<IFirebaseRealtimeDatabaseService, FirebaseRealtimeDatabaseService>();
            services.AddScoped<IFileStorageService, FirebaseStorageService>();
            services.AddScoped<ISlackService, SlackService>();
            #endregion

            #region DI_REPOSITORY
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();           
           #endregion
            //Redis Connection
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnectionString");
                options.InstanceName = "SampleInstance";
            });

            //Database Connection
            services.AddDbContext<ThinkTankContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultSQLConnection"));
            });
            services.AddAutoMapper(typeof(Mapping));
            services.AddScoped<IAuthorizationHandler, CustomAuthorizationHandler>();
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            return services;
        }
        public static IServiceCollection RegisterRequestHandlers(
           this IServiceCollection services)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
            }
            return services;
        }
    }
}