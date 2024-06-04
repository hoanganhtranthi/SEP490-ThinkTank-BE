

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.UnitOfWork;
using static ThinkTank.Domain.Enums.Enum;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Topics.Queries.GetTopics
{
    public class GetTopicsQueryHandler : IQueryHandler<GetTopicsQuery, PagedResults<TopicResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetTopicsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<TopicResponse>> Handle(GetTopicsQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var filter = _mapper.Map<TopicResponse>(request.TopicRequest);
                var topics = _unitOfWork.Repository<Topic>().GetAll().AsNoTracking().Include(a => a.Game)
                    .Select(a => new TopicResponse
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
                    }).DynamicFilter(filter).ToList();

                if (request.TopicRequest.IsHavingAsset == StatusTopicType.True)
                    topics = topics.Where(x => x.Assets.Count() > 0).ToList();

                if (request.TopicRequest.IsHavingAsset == StatusTopicType.False)
                    topics = topics.Where(x => x.Assets.Count() == 0).ToList();
                else topics = topics.ToList();

                var sort = PageHelper<TopicResponse>.Sorting(request.PagingRequest.SortType, topics, request.PagingRequest.ColName);
                var result = PageHelper<TopicResponse>.Paging(sort, request.PagingRequest.Page, request.PagingRequest.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get topic list error!!!!!", ex.Message);
            }
        }
    }
}
