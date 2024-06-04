
using System.ComponentModel.DataAnnotations;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Accounts.Queries.GetGameLevelByAccountId
{
    public class GetGameLevelByAccountIdQuery:IQuery<List<GameLevelOfAccountResponse>>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public GetGameLevelByAccountIdQuery(int id)
        {
            Id = id;
        }
    }
}
