

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Commands.UpdateAccountInRoom
{
    public class UpdateAccountInRoomCommand:ICommand<AccountInRoomResponse>
    {
        public string RoomCode { get; }
        public CreateAndUpdateAccountInRoomRequest CreateAndUpdateAccountInRoomRequest { get; }
        public UpdateAccountInRoomCommand(string roomCode, CreateAndUpdateAccountInRoomRequest createAndUpdateAccountInRoomRequest)
        {
            RoomCode = roomCode;
            CreateAndUpdateAccountInRoomRequest = createAndUpdateAccountInRoomRequest;
        }
    }
}
