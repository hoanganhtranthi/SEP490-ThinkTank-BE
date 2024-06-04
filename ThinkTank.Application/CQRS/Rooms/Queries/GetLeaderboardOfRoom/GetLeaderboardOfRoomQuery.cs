

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetLeaderboardOfRoom
{
    public class GetLeaderboardOfRoomQuery:IQuery<List<LeaderboardResponse>>
    {
        public string RoomCode { get; }
        public GetLeaderboardOfRoomQuery(string roomCode)
        {
            RoomCode = roomCode;
        }
    }
}
