using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MuTote.API.Mapper;
using MuTote.API.Utility;
using StackExchange.Redis;
using Repository.Extensions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using ThinkTank.Data.Entities;
using ThinkTank.Data.Repository;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.ImpService;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;
using Microsoft.AspNetCore.Builder.Extensions;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using ThinkTank.API.AppStart;
using FireSharp.Config;
using RedLockNet.SERedis;
using RedLockNet;
using RedLockNet.SERedis.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Mapping));
builder.Services.AddScoped<IFileStorageService, FirebaseStorageService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IIconService, IconService>();
builder.Services.AddScoped<IIconOfAccountService,IconOfAccountService>();
builder.Services.AddScoped<IAccountIn1vs1Service, AccountIn1vs1Service>();
builder.Services.AddScoped<IContestService, ContestService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<IFirebaseMessagingService, FirebaseMessagingService>();
builder.Services.AddScoped<IAccountInContestService, AccountInContestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRoomService,RoomService>();
builder.Services.AddScoped<ITypeOfAssetService, TypeOfAssetService>();
builder.Services.AddScoped<ITypeOfAssetInContestService, TypeOfAssetInContestService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAccountIn1vs1Service, AccountIn1vs1Service>();
builder.Services.AddScoped<IAccountInRoomService, AccountInRoomService>();
builder.Services.AddScoped<IFirebaseRealtimeDatabaseService, FirebaseRealtimeDatabaseService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddTransient<IAuthorizationHandler, CustomAuthorizationHandler>();

//FCM
System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "thinktank-ad0b3-45e7681d45c6.json");
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.GetApplicationDefault(),
    ProjectId = builder.Configuration.GetValue<string>("Firebase:ProjectId")
});

//Redis Connection
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
    options.InstanceName = "SampleInstance";
});

//Database Connection
builder.Services.AddDbContext<ThinkTankContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins",
        builder =>
        {
            builder
            //.WithOrigins(GetDomain())
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

//use hangfire
builder.Services.ConfigureHangfireServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
        "Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\n" +
        "Example: \"Bearer 12345abdcef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "oauth2",
            Name = "Bearer",
            In = ParameterLocation.Header,
        },
        new List<string>()
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
//start JWT
var key = builder.Configuration.GetValue<string>("ApiSetting:Secret");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
//end JWT

//Set policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
        policy.AddRequirements(new CustomRequirement());
    });
    options.AddPolicy("Player", policy =>
    {
        policy.RequireRole("Player");
        policy.AddRequirements(new CustomRequirement());
    });
    options.AddPolicy("All", policy =>
    {
        policy.RequireRole("Player","Admin");
        policy.AddRequirements(new CustomRequirement());
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ThinkTank.API1");
        options.RoutePrefix = String.Empty;
    });
}
app.UseHangfireDashboard();
app.UseMiddleware(typeof(GlobalErrorHandlingMiddleware));
app.UseHttpsRedirection();
app.UseCors("_myAllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
