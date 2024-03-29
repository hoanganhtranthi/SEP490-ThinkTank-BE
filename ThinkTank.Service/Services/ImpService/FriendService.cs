﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;
using static Google.Apis.Requests.BatchRequest;

namespace ThinkTank.Service.Services.ImpService
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;

        public FriendService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<FriendResponse> CreateFriend(CreateFriendRequest createFriendRequest)
        {
            try
            {
                if(createFriendRequest.AccountId1==createFriendRequest.AccountId2)
                    throw new CrudException(HttpStatusCode.BadRequest, "Add friend Invalid !!!", "");

                var friend = _mapper.Map<CreateFriendRequest, Friend>(createFriendRequest);
                var s = _unitOfWork.Repository<Account>().GetAll().Include(s=>s.ReportAccountId1Navigations)
                    .Include(s=>s.ReportAccountId2Navigations).SingleOrDefault(s => s.Id == createFriendRequest.AccountId1);
                if (s == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId1} is not found !!!", "");
                }
                var cus = _unitOfWork.Repository<Account>().Find(s => s.Id == createFriendRequest.AccountId2);
                if (cus == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $" Account Id {createFriendRequest.AccountId2} is not found !!!", "");
                }
                var acc = _unitOfWork.Repository<Friend>().Find(x => x.AccountId1 == createFriendRequest.AccountId1 && x.AccountId2==createFriendRequest.AccountId2
                || x.AccountId1==createFriendRequest.AccountId2 && x.AccountId2==createFriendRequest.AccountId1);
                if (acc != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This friendship has already !!!", "");
                if (_unitOfWork.Repository<Friend>().GetAll()
                    .Count(a => a.AccountId1 == createFriendRequest.AccountId1 || a.AccountId2 == createFriendRequest.AccountId2) > 100)
                    throw new CrudException(HttpStatusCode.BadRequest, $"Account Id {createFriendRequest.AccountId1} is full of friends !!!", "");
                friend.Status = false;
                await _unitOfWork.Repository<Friend>().CreateAsync(friend);
                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if(cus.Fcm != null)
                fcmTokens.Add(cus.Fcm);
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
                                                           new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank Community", Body = $"{s.FullName} sent you a friend request.", ImageUrl = s.Avatar }, data);
                #endregion            
                Notification notification = new Notification
                {
                    AccountId = cus.Id,
                    Avatar = s.Avatar,
                    DateTime = DateTime.Now,
                    Description = $"{s.FullName} sent you a friend request.",
                    Status = false,
                    Title= "ThinkTank Community"
                };
               await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<FriendResponse>(friend);
                rs.UserName1 = s.UserName;
                rs.Avatar1 = s.Avatar;
                rs.UserName2 = cus.UserName;
                rs.Avatar2 = cus.Avatar;
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
                Friend friend = _unitOfWork.Repository<Friend>().GetAll().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (friend == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id{id.ToString()}", "");
                }
                _unitOfWork.Repository<Friend>().Delete(friend);
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
                var response =  _unitOfWork.Repository<Friend>().GetAll().Include(x=>x.AccountId1Navigation).Include(x=>x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found friendship with id {id.ToString()}", "");
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
                if (request.Status == null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Please enter Status", "");
                var friends = _unitOfWork.Repository<Friend>().GetAll().Include(a=>a.AccountId1Navigation)
                    .Include(a=>a.AccountId2Navigation).Select(x=>new FriendResponse
                {
                    Id = x.Id,
                    AccountId1=x.AccountId1,
                    AccountId2=x.AccountId2,
                    Status=x.Status,
                    UserName1=x.AccountId1Navigation.UserName,
                    UserName2=x.AccountId2Navigation.UserName,
                    Avatar1 = x.AccountId1Navigation.Avatar,
                    Avatar2 = x.AccountId2Navigation.Avatar,
                    UserCode1=x.AccountId1Navigation.Code,
                    UserCode2=x.AccountId2Navigation.Code,
            }) .ToList();

                if (request.AccountId != null)
                {
                    friends = await GetFriendsByAccountId(request);
                }
                friends = friends.Where(a =>
                                            (string.IsNullOrEmpty(request.UserCode) ||
                                             ((!string.IsNullOrEmpty(a.UserCode1) && a.UserCode1.Contains(request.UserCode)) ||
                                              (!string.IsNullOrEmpty(a.UserCode2) && a.UserCode2.Contains(request.UserCode)))) &&
                                                (string.IsNullOrEmpty(request.UserName) ||
                                                ((!string.IsNullOrEmpty(a.UserName1) && a.UserName1.Contains(request.UserName)) ||
                                                (!string.IsNullOrEmpty(a.UserName2) && a.UserName2.Contains(request.UserName))))).ToList();
                if (request.Status != Helpers.Enum.StatusType.All)
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
        private async Task<List<FriendResponse>> GetFriendsByAccountId(FriendRequest request)
        {
            try
            {
                var response = new List<FriendResponse>();
                var accounts = _unitOfWork.Repository<Account>().GetAll().Include(x => x.FriendAccountId1Navigations)
                    .Include(x => x.FriendAccountId2Navigations).Where(a => a.Id != request.AccountId).ToList();
                var account= _unitOfWork.Repository<Account>().GetAll().Include(x => x.FriendAccountId1Navigations)
                    .Include(x => x.FriendAccountId2Navigations).SingleOrDefault(a => a.Id == request.AccountId);
                foreach (var acc in accounts)
                {
                    var friend = _unitOfWork.Repository<Friend>().Find(x => x.AccountId1 == request.AccountId && x.AccountId2 == acc.Id
                    || x.AccountId1 == acc.Id && x.AccountId2 == request.AccountId);
                    FriendResponse friendResponse = new FriendResponse();
                    friendResponse.Id = friend?.Id??0;
                    if (account.FriendAccountId2Navigations.SingleOrDefault(a => a.AccountId1 == acc.Id) != null)
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
                    friendResponse.Status= friend?.Status ?? null;
                    response.Add(friendResponse);
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get friendship list error!!!!!", ex.Message);
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
                Friend friend = _unitOfWork.Repository<Friend>().GetAll().Include(x => x.AccountId1Navigation).Include(x => x.AccountId2Navigation).FirstOrDefault(u => u.Id == id);

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
                #region send noti for account
                List<string> fcmTokens = new List<string>();
                if(friend.AccountId2Navigation.Fcm != null)
                fcmTokens.Add(friend.AccountId2Navigation.Fcm);
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
                    DateTime = DateTime.Now,
                    Status = false,
                    Description = $"{friend.AccountId2Navigation.FullName}  has agreed to be friends. ",
                    Title = "ThinkTank Community"
                };
                await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                if (friend.AccountId1Navigation.FriendAccountId1Navigations.Where(x=>x.Status==true).Count() + friend.AccountId1Navigation.FriendAccountId2Navigations.Where(x => x.Status == true).Count() == 15)
                    GetBadge(friend.AccountId1Navigation, "The friendliest");
                if (friend.AccountId2Navigation.FriendAccountId1Navigations.Where(x => x.Status == true).Count() + friend.AccountId2Navigation.FriendAccountId2Navigations.Where(x => x.Status == true).Count() == 15)
                    GetBadge(friend.AccountId2Navigation, "The friendliest");
                if (friend.AccountId1Navigation.Badges.Where(x => x.Status == true).Count() == 10)
                    acc1.Coin += 100;
                if (friend.AccountId2Navigation.Badges.Where(x => x.Status == true).Count() == 10)
                    acc2.Coin += 100;
                
                await _unitOfWork.Repository<Account>().Update(acc1,friend.AccountId1);
                await _unitOfWork.Repository<Account>().Update(acc2, friend.AccountId2);
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
        private async Task GetBadge(Account account, string name)
        {
            var badge = _unitOfWork.Repository<Badge>().GetAll().Include(x => x.Challenge).SingleOrDefault(x => x.AccountId == account.Id && x.Challenge.Name.Equals(name));
            var challage = _unitOfWork.Repository<Challenge>().Find(x => x.Name.Equals(name));
            var noti = _unitOfWork.Repository<Notification>().Find(x => x.Title == $"You have received {challage.Name} badge.");
            if (badge != null)
            {
                if (badge.CompletedLevel < challage.CompletedMilestone)
                    badge.CompletedLevel += 1;
                if (badge.CompletedLevel == challage.CompletedMilestone && noti ==null)
                {
                    badge.CompletedDate = DateTime.Now;
                    await _unitOfWork.Repository<Badge>().Update(badge, badge.Id);
                    #region send noti for account
                    List<string> fcmTokens = new List<string>();
                    if (account.Fcm != null)
                        fcmTokens.Add(account.Fcm);
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
                                                               new FirebaseAdmin.Messaging.Notification() { Title = "ThinkTank", Body = $"You have received {challage.Name} badge.", ImageUrl = challage.Avatar }, data);
                    #endregion
                    Notification notification = new Notification
                    {
                        AccountId = account.Id,
                        Avatar = challage.Avatar,
                        DateTime = DateTime.Now,
                        Description = $"You have received {challage.Name} badge.",
                        Status=false,
                        Title = "ThinkTank"
                    };
                    await _unitOfWork.Repository<Notification>().CreateAsync(notification);
                }
            }
            else
            {
                CreateBadgeRequest createBadgeRequest = new CreateBadgeRequest();
                createBadgeRequest.AccountId = account.Id;
                createBadgeRequest.CompletedLevel = 1;
                createBadgeRequest.ChallengeId = challage.Id;
                var b = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                b.Status = false;
                await _unitOfWork.Repository<Badge>().CreateAsync(b);
            }
        }
    }
}
