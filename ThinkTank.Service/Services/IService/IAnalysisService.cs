

using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IAnalysisService
    {
        Task<dynamic> GetAnalysisOfAccountId(int accountId);
        Task<dynamic> GetAnalysisOfAccountIdAndGameId(AnalysisRequest request);
        Task<dynamic> GetAnalysisOfMemoryTypeByAccountId(int accountId);
        Task<AnalysisAverageScoreResponse> GetAverageScoreAnalysis(int gameId, int userId);
    }
}
