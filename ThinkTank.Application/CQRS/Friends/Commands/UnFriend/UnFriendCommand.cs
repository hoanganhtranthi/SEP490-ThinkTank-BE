

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Friends.Commands.UnFriend
{
    public class UnFriendCommand:ICommand<FriendResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public UnFriendCommand(int id)
        {
            Id = id;
        }
    }
}
