

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;

namespace ThinkTank.Application.CQRS.Rooms.Commands.RemoveRoomPartyInRealtimeDatabase
{
    public class RemoveRoomPartyInRealtimeDatabaseCommand:ICommand<bool>
    {
        public string RoomCode { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int DelayTime { get; }
        public RemoveRoomPartyInRealtimeDatabaseCommand(string roomCode, int delayTime)
        {
            RoomCode = roomCode;
            DelayTime = delayTime;
        }
    }
}
