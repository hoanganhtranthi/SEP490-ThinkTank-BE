
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Friends.Commands.AddFriend
{
    public class AddFriendCommand:ICommand<FriendResponse>
    {
        public CreateFriendRequest CreateFriendRequest { get; }
        public AddFriendCommand(CreateFriendRequest createFriendRequest)
        {
            CreateFriendRequest = createFriendRequest;
        }
    }
}
