using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThinkTank.Application.Repository;
using ThinkTank.Application.Services.ImpService;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Infrastructures.DatabaseContext;
using ThinkTank.Infrastructures.Mapper;
using ThinkTank.Infrastructures.Repository;
using ThinkTank.Infrastructures.UnitOfWorkRepo;

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
            services.AddAutoMapper(typeof(Mapping));
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddDbContext<ThinkTankContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultSQLConnection"));
            });
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFirebaseMessagingService, FirebaseMessagingService>();          
            services.AddScoped<IAccountIn1vs1Service, AccountIn1vs1Service>();
            services.AddScoped<IFirebaseRealtimeDatabaseService, FirebaseRealtimeDatabaseService>();
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