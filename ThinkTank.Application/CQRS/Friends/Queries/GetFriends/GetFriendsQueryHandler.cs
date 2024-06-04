
using System.Linq.Dynamic.Core;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using Microsoft.EntityFrameworkCore;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Friends.Queries.GetFriends
{
    public class GetFriendsQueryHandler : IQueryHandler<GetFriendsQuery, PagedResults<FriendResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetFriendsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResults<FriendResponse>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var friends = await GetFriendsByAccountId((int)(request.FriendRequest.AccountId));
                var friendOfAccount = friends;

                if (!string.IsNullOrEmpty(request.FriendRequest.UserCode))
                {
                    friends = friends.Where(a => !string.IsNullOrEmpty(a.UserCode1) && a.UserCode1.Contains(request.FriendRequest.UserCode)
                                               || !string.IsNullOrEmpty(a.UserCode2) && a.UserCode2.Contains(request.FriendRequest.UserCode))
                                    .ToList();
                }

                if (!string.IsNullOrEmpty(request.FriendRequest.UserName))
                {
                    var friendResponses = friendOfAccount.Where(a => !string.IsNullOrEmpty(a.UserName1) && a.UserName1.Contains(request.FriendRequest.UserName)
                                               || !string.IsNullOrEmpty(a.UserName2) && a.UserName2.Contains(request.FriendRequest.UserName))
                                    .ToList();

                    if (!string.IsNullOrEmpty(request.FriendRequest.UserCode))
                    {
                        // Loại bỏ các tài khoản trùng lặp giữa friends và friendResponses
                        var distinctFriendResponses = friendResponses.Except(friends).ToList();

                        // Kết hợp friends và distinctFriendResponses
                        friends.AddRange(distinctFriendResponses);
                    }
                    else friends = friendResponses;
                }

                if (request.FriendRequest.Status != StatusType.All)
                {
                    bool? status = null;
                    if (request.FriendRequest.Status.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.FriendRequest.Status.ToString().ToLower());
                    }
                    friends = friends.Where(a => a.Status == status).ToList();
                }
                var sort = PageHelper<FriendResponse>.Sorting(request.PagingRequest.SortType, friends,request.PagingRequest.ColName);
                var result = PageHelper<FriendResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get friendship list error!!!!!", ex.Message);
            }
        }
        private async Task<List<FriendResponse>> GetFriendsByAccountId(int accountId)
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
        }
    }
