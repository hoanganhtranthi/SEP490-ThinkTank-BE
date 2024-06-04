using Microsoft.EntityFrameworkCore;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.Services.ImpService
{
    public class BadgesService : IBadgesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly DateTime date;

        public BadgesService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _notificationService = notificationService;
        }
        private async Task<List<Badge>> GetListBadgesCompleted(Account account)
        {
            var result = new List<Badge>();
            var badges = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).Where(x => x.AccountId == account.Id).ToList();
            if (badges.Any())
            {
                foreach (var badge in badges)
                {
                    if (badge.CompletedLevel == badge.Challenge.CompletedMilestone)
                        result.Add(badge);
                }
            }
            return result;
        }
        public async Task GetBadge(Account account, string name)
        {
            var result = await GetListBadgesCompleted(account);

            if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                if (badge != null && account.Status == true)
                {
                    if (badge.CompletedLevel < challage.CompletedMilestone)
                        badge.CompletedLevel += 1;
                    if (badge.CompletedLevel == challage.CompletedMilestone)
                    {
                        badge.CompletedDate = date;
                        #region send noti for account
                        List<string> fcmTokens = new List<string>();
                        if (account.Fcm != null)
                            fcmTokens.Add(account.Fcm);
                        await _notificationService.SendNotification(fcmTokens, $"You have received {challage.Name} badge.", "ThinkTank", challage.Avatar, account.Id);
                        #endregion
                    }
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                }
                else
                {
                    badge = new Badge();
                    badge.AccountId = account.Id;
                    badge.CompletedLevel = 1;
                    badge.ChallengeId = challage.Id;
                    badge.Status = false;
                    await _unitOfWork.Repository<Badge>().CreateAsync(badge);
                }

            }
        }
        public async Task GetBadge(List<Account> accounts, string name)
        {
            foreach (var account in accounts)
            {
                var result = await GetListBadgesCompleted(account);
                if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
                {
                    var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                    var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                    if (badge != null && account.Status == true)
                    {
                        if (badge.CompletedLevel < challage.CompletedMilestone)
                            badge.CompletedLevel += 1;
                        if (badge.CompletedLevel == challage.CompletedMilestone)
                        {
                            badge.CompletedDate = date;
                            #region send noti for account
                            List<string> fcmTokens = new List<string>();
                            if (account.Fcm != null)
                                fcmTokens.Add(account.Fcm);
                            await _notificationService.SendNotification(fcmTokens, $"You have received {challage.Name} badge.", "ThinkTank", challage.Avatar, account.Id);
                            #endregion
                        }
                        await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                    }
                    else
                    {
                        badge = new Badge();
                        badge.AccountId = account.Id;
                        badge.CompletedLevel = 1;
                        badge.ChallengeId = challage.Id;
                        badge.Status = false;
                        await _unitOfWork.Repository<Badge>().CreateAsync(badge);
                    }
                }
            }
        }
        public async Task GetPlowLordBadge(Account account, List<Achievement> list)
        {
            var result = await GetListBadgesCompleted(account);
            if (result.SingleOrDefault(x => x.Challenge.Name.Equals("Plow Lord")) == null)
            {
                var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals("Plow Lord"));
                var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals("Plow Lord"));
                if (badge != null && list.Count() == (badge.CompletedLevel + 1))
                {
                    await GetBadge(account, "Plow Lord");
                }
                if (badge == null)
                {
                    badge = new Badge();
                    badge.AccountId = account.Id;
                    badge.CompletedLevel = 1;
                    badge.ChallengeId = challage.Id;
                    badge.Status = false;
                    await _unitOfWork.Repository<Badge>().CreateAsync(badge);
                }
            }
        }
    }
}
