

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Queries.GetToStartRoom
{
    public class GetToStartRoomQuery:IQuery<RoomResponse>
    {
        public string RoomCode { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Time { get; }
        public GetToStartRoomQuery(string roomCode, int accountId, int time)
        {
            RoomCode = roomCode;
            AccountId = accountId;
            Time = time;
        }
    }
}
