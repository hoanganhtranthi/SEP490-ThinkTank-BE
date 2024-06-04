

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommand:ICommand<RoomResponse>
    {
        [Required]
        public string RoomCode { get; }
        public List<CreateAndUpdateAccountInRoomRequest> CreateAndUpdateAccountInRoomRequests { get; }
        public UpdateRoomCommand(string roomCode, List<CreateAndUpdateAccountInRoomRequest> createAndUpdateAccountInRoomRequests)
        {
            RoomCode = roomCode;
            CreateAndUpdateAccountInRoomRequests = createAndUpdateAccountInRoomRequests;
        }
    }
}
