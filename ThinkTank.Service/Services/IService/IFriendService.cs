

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IFriendService
    {
        Task<PagedResults<FriendResponse>> GetFriends(FriendRequest request, PagingRequest paging);
        Task<FriendResponse> GetFriendById(int id);
        Task<FriendResponse> CreateFriend(CreateFriendRequest createFriendRequest);
        Task<FriendResponse> GetToUpdateStatus(int id);
        Task<FriendResponse> DeleteFriendship(int id);
    }
}
