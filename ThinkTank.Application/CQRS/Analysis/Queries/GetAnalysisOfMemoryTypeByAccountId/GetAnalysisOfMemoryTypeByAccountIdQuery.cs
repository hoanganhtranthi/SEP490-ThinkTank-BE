

using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfMemoryTypeByAccountId
{
    public class GetAnalysisOfMemoryTypeByAccountIdQuery:IQuery<AnalysisOfMemoryTypeResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        public GetAnalysisOfMemoryTypeByAccountIdQuery(int accountId)
        {
            AccountId = accountId;
        }
    }
}
