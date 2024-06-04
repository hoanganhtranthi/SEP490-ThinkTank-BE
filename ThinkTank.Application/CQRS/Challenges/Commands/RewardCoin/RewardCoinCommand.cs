

using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Challenges.Commands.RewardCoin
{
    public class RewardCoinCommand:ICommand<List<ChallengeResponse>>
    {
        public int AccountId { get; }
        public int? ChallengeId { get; }    
        public RewardCoinCommand(int accountId, int? challengeId)
        {
            AccountId = accountId;
            ChallengeId = challengeId;
        }
    }
}
