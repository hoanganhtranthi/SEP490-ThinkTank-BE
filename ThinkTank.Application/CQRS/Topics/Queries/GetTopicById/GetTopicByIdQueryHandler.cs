
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Topics.Queries.GetTopicById
{
    public class GetTopicByIdQueryHandler : IQueryHandler<GetTopicByIdQuery, TopicResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetTopicByIdQueryHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<TopicResponse> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking()
                    .Include(c => c.Game).Where(a => a.Id == request.Id).Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name,
                        GameId = a.GameId,
                        Assets = new List<AssetResponse>(a.Assets.Select(x => new AssetResponse
                        {
                            Id = x.Id,
                            GameId = x.Topic.GameId,
                            GameName = x.Topic.Game.Name,
                            Status = x.Status,
                            TopicId = a.Id,
                            TopicName = a.Name,
                            Value = x.Value,
                            Version = x.Version,
                            Answer = x.Topic.GameId == 2 ? System.IO.Path.GetFileName(new Uri(x.Value).LocalPath) : null
                        }))
                    }).SingleOrDefault();

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found topic with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get Topic By ID Error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Topic By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
