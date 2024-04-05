using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;

namespace ThinkTank.Service.Services.IService
{
    public interface IAnalysisService
    {
        Task<dynamic> GetAnalysisOfAccountId(int accountId);
        Task<dynamic> GetAnalysisOfAccountIdAndGameId(AnalysisRequest request);
        Task<dynamic> GetAnalysisOfMemoryTypeByAccountId(int accountId);
    }
}
