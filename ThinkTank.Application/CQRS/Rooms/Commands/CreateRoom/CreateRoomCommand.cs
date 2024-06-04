

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommand:ICommand<RoomResponse>
    {
        public CreateRoomRequest CreateRoomRequest { get; }
        public CreateRoomCommand(CreateRoomRequest createRoomRequest)
        {
            CreateRoomRequest = createRoomRequest;
        }
    }
}
