using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAnalysisService
    {
        Task<dynamic> GetAnalysisOfAccountId(int accountId);
        Task<dynamic> GetAnalysisOfAccountIdAndGameId(AnalysisRequest request);
        Task<dynamic> GetAnalysisOfMemoryTypeByAccountId(int accountId);
        Task<AnalysisAverageScoreResponse> GetAverageScoreAnalysis(int gameId, int userId);
    }
}
