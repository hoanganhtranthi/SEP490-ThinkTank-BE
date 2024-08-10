
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Games.Queries.GetGames
{
    public class GetGamesQueryHandler : IQueryHandler<GetGamesQuery, PagedResults<GameResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetGamesQueryHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<PagedResults<GameResponse>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var games = _unitOfWork.Repository<Game>().GetAll().AsNoTracking().Include(x => x.Topics).Select(x => new GameResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    AmoutPlayer = _unitOfWork.Repository<Achievement>().GetAll().AsNoTracking().Include(x => x.Game).Where(a => a.GameId == x.Id).Select(a => a.AccountId).Distinct().Count(),
                    Topics = new List<TopicResponse>(x.Topics.Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name
                    }))
                }).ToList();

                var sort = PageHelper<GameResponse>.Sorting(request.PagingRequest.SortType, games, request.PagingRequest.ColName);
                var result = PageHelper<GameResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get game list error!!!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get game list error!!!!!", ex.Message);
            }
        }
    }
}
