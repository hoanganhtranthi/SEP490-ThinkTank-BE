using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.Services.IService
{
    public interface IAnalysisService
    {
        Task<dynamic> GetAnalysisOfAccountId(int accountId);
        Task<dynamic> GetAnalysisOfAccountIdAndGameId(int accountId, int gameId);
    }
}
