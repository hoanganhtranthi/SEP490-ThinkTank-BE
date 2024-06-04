

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.FindAccountTo1vs1
{
    public class FindAccountTo1vs1Command:ICommand<RoomIn1vs1Response>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Coin { get; }
        public FindAccountTo1vs1Command(int gameId, int accountId, int coin)
        {
            GameId = gameId;
            AccountId = accountId;
            Coin = coin;
        }
    }
}
