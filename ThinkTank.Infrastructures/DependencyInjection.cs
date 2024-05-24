
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using ThinkTank.Application.ImpService;
using ThinkTank.Application.Repository;
using ThinkTank.Application.Services.ImpService;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Infrastructures.DatabaseContext;
using ThinkTank.Infrastructures.Mapper;
using ThinkTank.Infrastructures.Repository;
using ThinkTank.Infrastructures.UnitOfWorkRepo;

namespace ThinkTank.Infrastructures
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, IConfiguration configuration)
        {
            #region DI_SERVICES
            services.AddScoped<IFileStorageService, FirebaseStorageService>();            
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IAchievementService, AchievementService>();
            services.AddScoped<IIconService, IconService>();
            services.AddScoped<IIconOfAccountService, IconOfAccountService>();
            services.AddScoped<IAccountIn1vs1Service, AccountIn1vs1Service>();
            services.AddScoped<IContestService, ContestService>();
            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<IFirebaseMessagingService, FirebaseMessagingService>();
            services.AddScoped<IAccountInContestService, AccountInContestService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<ITypeOfAssetService, TypeOfAssetService>();
            services.AddScoped<ITypeOfAssetInContestService, TypeOfAssetInContestService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAccountIn1vs1Service, AccountIn1vs1Service>();
            services.AddScoped<IAccountInRoomService, AccountInRoomService>();
            services.AddScoped<IFirebaseRealtimeDatabaseService, FirebaseRealtimeDatabaseService>();
            services.AddScoped<IAnalysisService, AnalysisService>();          
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
    }
}
