

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.CQRS.Friends.UpdateStatusFriendship;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Friends.Commands.UpdateStatusFriendship
{
    public class UpdateStatusFriendshipCommandHandler : ICommandHandler<UpdateStatusFriendshipCommand, FriendResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly INotificationService _notificationService;
        private readonly IBadgesService _badgesService;
        private readonly ISlackService _slackService;
        public UpdateStatusFriendshipCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
            INotificationService notificationService,IBadgesService badgesService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _notificationService = notificationService;
            _badgesService = badgesService;
            _slackService = slackService;
        }

        public async Task<FriendResponse> Handle(UpdateStatusFriendshipCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Friend friend = _unitOfWork.Repository<Friend>().GetAll()
                    .Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == request.Id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{request.Id.ToString()}", "");
                }
                var acc1 = _unitOfWork.Repository<Account>().Find(x => x.Id == friend.AccountId1);
                var acc2 = _unitOfWork.Repository<Account>().Find(x => x.Id == friend.AccountId2);

                friend.Status = true;

                await _unitOfWork.Repository<Friend>().Update(friend, request.Id);

                if (friend.AccountId2Navigation.Avatar == null)
                    friend.AccountId2Navigation.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";

                if (friend.AccountId1Navigation.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");

                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if (friend.AccountId1Navigation.Fcm != null)
                    fcmTokens.Add(friend.AccountId1Navigation.Fcm);
                await _notificationService.SendNotification(fcmTokens, $"{friend.AccountId2Navigation.FullName}  has agreed to be friends. ", "ThinkTank Community", friend.AccountId2Navigation.Avatar, friend.AccountId1);
                #endregion

                await _badgesService.GetBadge(new List<Account> { acc1, acc2 }, "The friendliest");

                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = friend.AccountId1Navigation.UserName;
                rs.UserName2 = friend.AccountId2Navigation.UserName;
                rs.Avatar1 = friend.AccountId1Navigation.Avatar;
                rs.Avatar2 = friend.AccountId2Navigation.Avatar;

                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Accept friend error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Accept friend error!!!!!", ex.Message);
            }
        }
    }
}
