

namespace ThinkTank.Application.DTO.Response
{
    public class AnalysisAverageScoreResponse
    {
        public int UserId { get; set; }
        public int CurrentLevel { get; set; }
        public List<AnalysisAverageScoreByGroupLevelResponse> AnalysisAverageScoreByGroupLevelResponses { get; set; }
       
    }
}
