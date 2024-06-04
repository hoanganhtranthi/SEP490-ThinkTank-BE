

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.Services.ImpService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime date;
        private readonly IFirebaseMessagingService _firebaseMessagingService;

        public NotificationService(IUnitOfWork unitOfWork, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _firebaseMessagingService = firebaseMessagingService;
        }
        public async Task SendNotification(List<string> fcms, string message, string title, string imgUrl, List<int> ids)
        {
            var data = new Dictionary<string, string>()
            {
                ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                ["Action"] = "home",
                ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                }),
            };
            if (fcms.Any())
                _firebaseMessagingService.SendToDevices(fcms,
                                                       new FirebaseAdmin.Messaging.Notification() { Title = title, Body = $"{message}", ImageUrl = imgUrl }, data);
            foreach (var id in ids)
            {
                Notification notification = new Notification
                {
                    AccountId = id,
                    Avatar = imgUrl,
                    DateNotification = date,
                    Status = false,
                    Description = $"{message}",
                    Title = title
                };

                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
            }
        }
        public async Task SendNotification(List<string> fcms, string message, string title, string imgUrl, int id)
        {
            var data = new Dictionary<string, string>()
            {
                ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                ["Action"] = "home",
                ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                }),
            };
            if (fcms.Any())
                _firebaseMessagingService.SendToDevices(fcms,
                                                       new FirebaseAdmin.Messaging.Notification() { Title = title, Body = $"{message}", ImageUrl = imgUrl }, data);
                Notification notification = new Notification
                {
                    AccountId = id,
                    Avatar = imgUrl,
                    DateNotification = date,
                    Status = false,
                    Description = $"{message}",
                    Title = title,

                };

                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
        }       
    }
}
