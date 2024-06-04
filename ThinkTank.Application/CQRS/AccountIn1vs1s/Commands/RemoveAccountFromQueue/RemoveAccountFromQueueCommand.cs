

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Queries.RemoveAccountFromQueue
{
    public class RemoveAccountFromQueueCommand:IQuery<bool>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Coin { get; }
        public string UniqueId { get; }
        public  int DelayTime { get; }
        public RemoveAccountFromQueueCommand(int accountId, int coin, string uniqueId, int delayTime,int gameId)
        {
            AccountId = accountId;
            Coin = coin;
            UniqueId = uniqueId;
            DelayTime = delayTime;
            GameId = gameId;
        }   
    }
}
