

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Analysis.Queries.GetAverageScoreAnalysis
{
    public class GetAverageScoreAnalysisQuery:IQuery<AnalysisAverageScoreResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int GameId { get; }
        public GetAverageScoreAnalysisQuery(int accountId, int gameId)
        {
            AccountId = accountId;
            GameId = gameId;
        }
    }
}
