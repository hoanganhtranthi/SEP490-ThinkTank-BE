using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IRoomService
    {
        Task<PagedResults<RoomResponse>> GetRooms(RoomRequest request, PagingRequest paging);
        Task<RoomResponse> CreateRoom(CreateRoomRequest createRoomRequest);
        Task<RoomResponse> GetRoomById(int id);
        Task<List<LeaderboardResponse>> GetLeaderboardOfRoom(int roomId);
        Task<RoomResponse> DeleteRoom(int roomId, int accountId);
        Task<RoomResponse> GetToUpdateStatusRoom(int roomId);
        Task<RoomResponse> UpdateRoom(int roomId, List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests);
    }
}
