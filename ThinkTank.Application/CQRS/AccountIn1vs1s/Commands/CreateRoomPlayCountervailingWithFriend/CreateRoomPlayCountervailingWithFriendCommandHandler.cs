

using AutoMapper;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateRoomPlayCountervailingWithFriend
{
    public class CreateRoomPlayCountervailingWithFriendCommandHandler : ICommandHandler<CreateRoomPlayCountervailingWithFriendCommand, RoomIn1vs1Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public readonly IBadgesService _badgesService;
        private readonly DateTime date;
        private readonly INotificationService _notificationService;
        public CreateRoomPlayCountervailingWithFriendCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBadgesService badgesService,INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _badgesService = badgesService;
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _notificationService = notificationService;
        }

        public async Task<RoomIn1vs1Response> Handle(CreateRoomPlayCountervailingWithFriendCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var game = _unitOfWork.Repository<Game>().Find(u => u.Id == request.GameId);

                if (game == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id {request.GameId}", "");
                }

                var account1 = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId1);
                if (account1 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId1} Not Found!!!!!", "");
                }
                if (account1.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId1} Not Available!!!!!", "");
                }

                var account2 = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId2);
                if (account2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Account Id {request.AccountId2} Not Found!!!!!", "");
                }
                if (account2.Status.Equals(false))
                {
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId2}  Not Available!!!!!", "");
                }


                var friend = _unitOfWork.Repository<Friend>().Find(x => x.AccountId2 == request.AccountId2 && x.AccountId1 == request.AccountId1 && x.Status == true
                || x.AccountId1 == request.AccountId1 && x.AccountId2 == request.AccountId2 && x.Status == true);
                if (friend == null)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.AccountId1} and account id {request.AccountId2} is not friend so can not play 1vs1 together", "");

                var uniqueId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if (account2.Fcm != null)
                    fcmTokens.Add(account2.Fcm);
                await _notificationService.SendNotification(fcmTokens, $"You receive an invitation to play countervailing mode from your friend {account1.FullName}"
                    , $"ThinkTank Countervailing With Friend {uniqueId}/{request.GameId}", account1.Avatar, account2.Id);

                #endregion
                await _unitOfWork.CommitAsync();
                return new RoomIn1vs1Response
                {
                    AccountId = account2.Id,
                    Avatar = account2.Avatar,
                    Coin = (int)account2.Coin,
                    RoomId = uniqueId,
                    Username = account2.UserName
                };
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Match play countervailing mode with friend error !!!!!", ex.Message);
            }
        }
    }
}
