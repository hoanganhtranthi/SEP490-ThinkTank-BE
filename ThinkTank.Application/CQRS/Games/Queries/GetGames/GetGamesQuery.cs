
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Games.Queries.GetGames
{
    public class GetGamesQuery : IGetTsQuery<PagedResults<GameResponse>>
    {
        public GetGamesQuery(PagingRequest pagingRequest) : base(pagingRequest)
        {
        }
    }
}
