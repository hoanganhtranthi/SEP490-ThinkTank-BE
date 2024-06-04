
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Friends.Queries.GetFriends
{
    public class GetFriendsQuery:IGetTsQuery<PagedResults<FriendResponse>>
    {
        public FriendRequest FriendRequest { get; }
        public GetFriendsQuery(PagingRequest pagingRequest,FriendRequest friendRequest) : base(pagingRequest)
        {
            FriendRequest = friendRequest;
        }
       
    }
}
