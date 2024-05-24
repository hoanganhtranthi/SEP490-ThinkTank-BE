using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Domain.Entities;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Application.Helpers;

namespace ThinkTank.Application.Services.ImpService
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly DateTime date;
        private readonly IFirebaseMessagingService _firebaseMessagingService;

        public FriendService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromHours(7))
                date = DateTime.UtcNow.ToLocalTime().AddHours(7);
            else date = DateTime.Now;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<FriendResponse> CreateFriend(CreateFriendRequest createFriendRequest)
        {
            try
            {
                if(createFriendRequest.AccountId1==createFriendRequest.AccountId2 || createFriendRequest.AccountId1 <=0 || createFriendRequest.AccountId2 <=0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Add friend Invalid !!!", "");

                var friend = _mapper.Map<CreateFriendRequest, Friend>(createFriendRequest);

                var acc1 = _unitOfWork.Repository<Account>().Find(s => s.Id == createFriendRequest.AccountId1);
                if (acc1 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId1} is not found !!!", "");
                }

                if (acc1.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc1.Id} is block", "");

                var acc2 = _unitOfWork.Repository<Account>().Find(s => s.Id == createFriendRequest.AccountId2);
                if (acc2 == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId2} is not found !!!", "");
                }
                if (acc2.Status == false) throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {acc2.Id} is block", "");

                var friendOfAccount = _unitOfWork.Repository<Friend>().Find(x => x.AccountId1 == createFriendRequest.AccountId1 && x.AccountId2==createFriendRequest.AccountId2
                || x.AccountId1==createFriendRequest.AccountId2 && x.AccountId2==createFriendRequest.AccountId1);
                
                if (friendOfAccount != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This friendship has already !!!", "");
                
                if (_unitOfWork.Repository<Friend>().GetAll()
                    .Count(a => a.AccountId1 == createFriendRequest.AccountId1 || a.AccountId2 == createFriendRequest.AccountId2) > 100)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createFriendRequest.AccountId1} is full of friends !!!", "");
                
                friend.Status = false;
                await _unitOfWork.Repository<Friend>().CreateAsync(friend);

                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if(acc2.Fcm != null)
                fcmTokens.Add(acc2.Fcm);
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
                if (fcmTokens.Any())
                    _firebaseMessagingService.SendToDevices(fcmTokens,
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Community", Body = $"{acc1.FullName} sent you a friend request.", ImageUrl = acc1.Avatar }, data);
                #endregion            
               
                Notification notification = new Notification
                {
                    AccountId = acc2.Id,
                    Avatar = acc1.Avatar,
                    DateNotification = date,
                    Description = $"{acc1.FullName} sent you a friend request.",
                    Status = false,
                    Title= "ThinkTank Community"
                };
                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Friend Error!!!", ex?.Message);
            }
        }

        public async Task<FriendResponse> DeleteFriendship(int id)
        {
            try
            {
                if (id <=0 )
                    throw new CrudException(HttpStatusCode.BadRequest, "Information is invalid", "");

                Friend friend = _unitOfWork.Repository<Friend>().GetAll()
                    .Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{id}", "");
                }
                await _unitOfWork.Repository<Friend>().RemoveAsync(friend);
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete friendship error!!!!!", ex.Message);
            }
        }

        public async Task<FriendResponse> GetFriendById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Friendship Invalid", "");
                }
                var response =  _unitOfWork.Repository<Friend>().GetAll()
                .AsNoTracking().Include(x=>x.AccountId1Navigation).Include(x=>x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id {id}", "");
                }

                var rs= _mapper.Map<FriendResponse>(response);
                rs.UserName1 = response.AccountId1Navigation.UserName;
                rs.UserName2 = response.AccountId2Navigation.UserName;
                rs.Avatar1 = response.AccountId1Navigation.Avatar;
                rs.Avatar2 = response.AccountId2Navigation.Avatar;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Friendship By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public  async Task<PagedResults<FriendResponse>> GetFriends(FriendRequest request, PagingRequest paging)
        {
            try
            {
                var friends = await GetFriendsByAccountId((int)request.AccountId);
                var friendOfAccount = friends;

                if (!string.IsNullOrEmpty(request.UserCode))
                {
                    friends = friends.Where(a => !string.IsNullOrEmpty(a.UserCode1) && a.UserCode1.Contains(request.UserCode)
                                               || !string.IsNullOrEmpty(a.UserCode2) && a.UserCode2.Contains(request.UserCode))
                                    .ToList();
                }

                if (!string.IsNullOrEmpty(request.UserName))
                {
                    var friendResponses = friendOfAccount.Where(a => !string.IsNullOrEmpty(a.UserName1) && a.UserName1.Contains(request.UserName)
                                               || !string.IsNullOrEmpty(a.UserName2) && a.UserName2.Contains(request.UserName))
                                    .ToList();

                    if (!string.IsNullOrEmpty(request.UserCode))
                    {
                        // Loại bỏ các tài khoản trùng lặp giữa friends và friendResponses
                        var distinctFriendResponses = friendResponses.Except(friends).ToList();

                        // Kết hợp friends và distinctFriendResponses
                        friends.AddRange(distinctFriendResponses);
                    }
                    else friends = friendResponses;
                }

                if (request.Status != StatusType.All)
                {
                    bool? status = null;
                    if (request.Status.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.Status.ToString().ToLower());
                    }
                    friends = friends.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<FriendResponse>.Sorting(paging.SortType, friends, paging.ColName);
                var result = PageHelper<FriendResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get friendship list error!!!!!", ex.Message);
            }
        }
        private async Task<List<FriendResponse>> GetFriendsByAccountId(int accountId)
        {
            try
            {
                var response = new List<FriendResponse>();
                var accountsWithFriends = _unitOfWork.Repository<Account>().GetAll().AsNoTracking()
                    .Include(a => a.FriendAccountId1Navigations)
                    .Include(a => a.FriendAccountId2Navigations)
                    .Where(a => a.Id != accountId)
                    .ToList();

                var currentAccount = _unitOfWork.Repository<Account>().GetAll().AsNoTracking()
                    .Include(a => a.FriendAccountId1Navigations)
                    .Include(a => a.FriendAccountId2Navigations)
                    .SingleOrDefault(a => a.Id == accountId);

                foreach (var acc in accountsWithFriends)
                {
                    var friend = acc.FriendAccountId1Navigations
                        .Concat(acc.FriendAccountId2Navigations)
                        .SingleOrDefault(f => (f.AccountId1 == accountId && f.AccountId2 == acc.Id)
                        || (f.AccountId1 == acc.Id && f.AccountId2 == accountId));

                    FriendResponse friendResponse = new FriendResponse();
                    friendResponse.Id = friend?.Id ?? 0;

                    if (currentAccount.FriendAccountId2Navigations.Any(f => f.AccountId1 == acc.Id))
                    {
                        friendResponse.AccountId1 = acc.Id;
                        friendResponse.Avatar1 = acc.Avatar;
                        friendResponse.UserCode1 = acc.Code;
                        friendResponse.UserName1 = acc.UserName;
                    }
                    else
                    {
                        friendResponse.AccountId2 = acc.Id;
                        friendResponse.Avatar2 = acc.Avatar;
                        friendResponse.UserCode2 = acc.Code;
                        friendResponse.UserName2 = acc.UserName;
                    }
                    friendResponse.Status = friend?.Status;
                    response.Add(friendResponse);
                }

                return response;

            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get friendship list of account error!!!!!", ex.Message);
            }
        }

        public async Task<FriendResponse> GetToUpdateStatus(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Friendship Invalid", "");
                }
                Friend friend = _unitOfWork.Repository<Friend>().GetAll()
                    .Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{id.ToString()}", "");
                }
                var acc1 = _unitOfWork.Repository<Account>().Find(x => x.Id == friend.AccountId1);
                var acc2 = _unitOfWork.Repository<Account>().Find(x => x.Id == friend.AccountId2);

                friend.Status = true;

                await _unitOfWork.Repository<Friend>().Update(friend, id);

                if (friend.AccountId2Navigation.Avatar == null)
                    friend.AccountId2Navigation.Avatar = "https://firebasestorage.googleapis.com/v0/b/thinktank-79ead.appspot.com/o/System%2Favatar-trang-4.jpg?alt=media&token=2ab24327-c484-485a-938a-ed30dc3b1688";
                
                if (friend.AccountId1Navigation.Status == false) throw new CrudException(HttpStatusCode.BadRequest, "Your account is block", "");
                
                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if(friend.AccountId1Navigation.Fcm != null)
                fcmTokens.Add(friend.AccountId1Navigation.Fcm);
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
                if (fcmTokens.Any())
                    _firebaseMessagingService.SendToDevices(fcmTokens,
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Community", Body = $"{friend.AccountId2Navigation.FullName}  has agreed to be friends. ", ImageUrl = friend.AccountId2Navigation.Avatar }, data);
                #endregion          
                Notification notification = new Notification
                {
                    AccountId = friend.AccountId1,
                    Avatar = friend.AccountId2Navigation.Avatar,
                    DateNotification = date,
                    Status = false,
                    Description = $"{friend.AccountId2Navigation.FullName}  has agreed to be friends. ",
                    Title = "ThinkTank Community"
                };

                await _unitOfWork.Repository<Notification>().CreateAsync(notification);

                await GetBadge(new List<Account> { acc1,acc2},"The friendliest");  
                
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Accept friend error!!!!!", ex.Message);
            }
        }
        private async Task<List<Badge>> GetBadgeCompleted(Account account)
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
        private async Task GetBadge(List<Account> accounts, string name)
        {
            foreach (var account in accounts)
            {
                var result = await GetBadgeCompleted(account);
                if (result.SingleOrDefault(x => x.Challenge.Name.Equals(name)) == null)
                {
                    var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
                    var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
                    if (badge != null && account.Status==true)
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
                            var data = new Dictionary<string, string>()
                            {
                                ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                                ["Action"] = "/achievement",
                                ["Argument"] = JsonConvert.SerializeObject(new JsonSerializerSettings
                                {
                                    ContractResolver = new DefaultContractResolver
                                    {
                                        NamingStrategy = new SnakeCaseNamingStrategy()
                                    }
                                }),
                            };
                            if (fcmTokens.Any())
                                _firebaseMessagingService.SendToDevices(fcmTokens,
                                                                       new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank", Body = $"You have received {challage.Name} badge.", ImageUrl = challage.Avatar }, data);
                            #endregion
                            Notification notification = new Notification
                            {
                                AccountId = account.Id,
                                Avatar = challage.Avatar,
                                DateNotification = date,
                                Description = $"You have received {challage.Name} badge.",
                                Status = false,
                                Title = "ThinkTank"
                            };
                            await _unitOfWork.Repository<Notification>().CreateAsync(notification);
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
    }
}
