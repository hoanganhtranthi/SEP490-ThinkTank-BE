
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Games.Queries.GetGameById
{
    public class GetGameByIdQuery : IGetTByIdQuery<GameResponse>
    {
        public GetGameByIdQuery(int id) : base(id)
        {
        }
    }
}
