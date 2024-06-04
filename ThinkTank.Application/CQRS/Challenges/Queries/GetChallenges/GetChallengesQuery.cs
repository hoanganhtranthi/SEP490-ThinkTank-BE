

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Challenges.Queries.GetChallenges
{
    public class GetChallengesQuery:IQuery<List<ChallengeResponse>>
    {
        public ChallengeRequest ChallengeRequest { get; }
        public GetChallengesQuery(ChallengeRequest challengeRequest)
        {
            ChallengeRequest = challengeRequest;
        }
    }
}
