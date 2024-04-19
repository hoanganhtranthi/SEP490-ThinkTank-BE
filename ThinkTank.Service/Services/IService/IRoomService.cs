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
        Task<List<LeaderboardResponse>> GetLeaderboardOfRoom(string roomCode);
        Task<RoomResponse> DeleteRoom(int roomId, int accountId);      
        Task<RoomResponse> LeaveRoom(int roomId, int accountId);
        Task<RoomResponse> GetToStartRoom(string roomCode, int accountId, int time);
        Task<RoomResponse> UpdateRoom(string roomCode, List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests);
    }
}
