

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Commands.LeaveRoom
{
    public class LeaveRoomCommand:ICommand<RoomResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int RoomId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        public LeaveRoomCommand(int roomId, int accountId)
        {
            RoomId = roomId;
            AccountId = accountId;
        }
    }
}
