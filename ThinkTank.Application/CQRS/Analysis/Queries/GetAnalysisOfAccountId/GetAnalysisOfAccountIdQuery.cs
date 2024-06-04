
using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Analysis.Queries.GetAnalysisOfAccountId 
{ 
    public class GetAnalysisOfAccountIdQuery:IQuery<AdminDashboardResponse>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int AccountId { get; }
        public GetAnalysisOfAccountIdQuery(int accountId)
        {
            AccountId = accountId;
        }
    }
}
