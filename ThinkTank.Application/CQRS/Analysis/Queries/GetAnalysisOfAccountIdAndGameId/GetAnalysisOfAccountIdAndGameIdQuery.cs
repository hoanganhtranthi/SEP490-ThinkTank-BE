

using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountIdAndGameId
{
    public class GetAnalysisOfAccountIdAndGameIdQuery:IQuery<List<RatioMemorizedDailyResponse>>
    {
        public AnalysisRequest AnalysisRequest { get; }
        public GetAnalysisOfAccountIdAndGameIdQuery(AnalysisRequest analysisRequest)
        {
            AnalysisRequest = analysisRequest;
        }
    }
}
