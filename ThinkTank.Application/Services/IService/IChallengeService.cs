

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IChallengeService
    {
        Task<List<ChallengeResponse>> GetChallenges(ChallengeRequest request);
        Task<List<ChallengeResponse>> GetCoinReward(int accountId, int? challengeId);
    }
}
