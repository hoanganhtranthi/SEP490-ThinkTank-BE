

using AutoMapper;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.Application.CQRS.Friends.Commands.AddFriend
{
    public class AddFriendCommandHandler : ICommandHandler<AddFriendCommand, FriendResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ISlackService _slackService;
        public AddFriendCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _slackService = slackService;
        }
        public async Task<FriendResponse> Handle(AddFriendCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CreateFriendRequest.AccountId1 == request.CreateFriendRequest.AccountId2 || request.CreateFriendRequest.AccountId1 <= 0 || request.CreateFriendRequest.AccountId2 <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Add friend Invalid !!!", "");
                var friend = _mapper.Map<CreateFriendRequest, Friend>(request.CreateFriendRequest);

                var acc1 = _unitOfWork.Repository<Account>().Find(s => s.Id == request.CreateFriendRequest.AccountId1);
                if (acc1 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {request.CreateFriendRequest.AccountId1} is not found !!!", "");
                }

                if (acc1.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc1.Id} is block", "");

                var acc2 = _unitOfWork.Repository<Account>().Find(s => s.Id == request.CreateFriendRequest.AccountId2);
                if (acc2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {request.CreateFriendRequest.AccountId2} is not found !!!", "");
                }
                if (acc2.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc2.Id} is block", "");

                var friendOfAccount = _unitOfWork.Repository<Friend>().Find(x => x.AccountId1 == request.CreateFriendRequest.AccountId1 && x.AccountId2 == request.CreateFriendRequest.AccountId2
                || x.AccountId1 == request.CreateFriendRequest.AccountId2 && x.AccountId2 == request.CreateFriendRequest.AccountId1);
                if (friendOfAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This friendship has already !!!", "");
                if (_unitOfWork.Repository<Friend>().GetAll()
                    .Count(a => a.AccountId1 == request.CreateFriendRequest.AccountId1 || a.AccountId2 == request.CreateFriendRequest.AccountId2) > 100)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {request.CreateFriendRequest.AccountId1} is full of friends !!!", "");

                friend.Status = false;
                await _unitOfWork.Repository<Friend>().CreateAsync(friend);

                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if (acc2.Fcm != null)
                    fcmTokens.Add(acc2.Fcm);
                await _notificationService.SendNotification(fcmTokens, $"{acc1.FullName} sent you a friend request.", "ThinkTank Community", acc1.Avatar, acc2.Id);
                #endregion

                await _unitOfWork.CommitAsync();

                var rs = _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = acc1.UserName;
                rs.Avatar1 = acc1.Avatar;
                rs.UserName2 = acc2.UserName;
                rs.Avatar2 = acc2.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Add Friend Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Friend Error!!!", ex?.Message);
            }
        }
    }
}

