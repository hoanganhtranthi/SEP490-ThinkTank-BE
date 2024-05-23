
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
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
        Task<bool> RemoveRoomPartyInRealtimeDatabase(string roomCode, int delayTime);
        Task<RoomResponse> UpdateRoom(string roomCode, List<CreateAndUpdateAccountInRoomRequest> createAccountInRoomRequests);
    }
}
