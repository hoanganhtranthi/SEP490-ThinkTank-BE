

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateRoomPlayCountervailingWithFriend
{
    public class CreateRoomPlayCountervailingWithFriendCommand : ICommand<RoomIn1vs1Response>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId1 { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId2 { get; }
        public CreateRoomPlayCountervailingWithFriendCommand(int gameId, int accountId1, int accountId2)
        {
            GameId = gameId;
            AccountId1 = accountId1;
            AccountId2 = accountId2;
        }
    }
}
