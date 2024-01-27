using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
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
