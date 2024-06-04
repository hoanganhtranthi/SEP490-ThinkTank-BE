

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Friends.UpdateStatusFriendship
{
    public class UpdateStatusFriendshipCommand:ICommand<FriendResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public UpdateStatusFriendshipCommand(int id) { Id = id; }
    }
}
