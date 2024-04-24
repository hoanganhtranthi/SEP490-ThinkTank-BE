

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MuTote.API.Mapper;
using Repository.Extensions;
using ThinkTank.Data.Entities;
using ThinkTank.Data.Repository;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.ImpService;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Test
{
    public abstract class BaseTest
    {
        protected ThinkTankContext Context { get; private set; } = null!;
        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            services.AddDbContext<ThinkTankContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultSQLConnection"));
            });
            services.AddAutoMapper(typeof(Mapping));
            services.AddScoped<IFileStorageService, FirebaseStorageService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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
            services.AddScoped<IAuthorizationHandler, CustomAuthorizationHandler>();

            ServiceProvider = services.BuildServiceProvider();
            Context = ServiceProvider.GetRequiredService<ThinkTankContext>();
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }
    }
}