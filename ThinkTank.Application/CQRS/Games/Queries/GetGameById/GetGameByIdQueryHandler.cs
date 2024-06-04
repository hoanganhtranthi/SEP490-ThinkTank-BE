

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Games.Queries.GetGameById
{
    public class GetGameByIdQueryHandler : IQueryHandler<GetGameByIdQuery, GameResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetGameByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GameResponse> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Game>().GetAll().AsNoTracking().Include(x => x.Topics).Select(x => new GameResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    AmoutPlayer = _unitOfWork.Repository<Achievement>().GetAll().Include(x => x.Game).Where(x => x.GameId == request.Id).Select(a => a.AccountId).Distinct().Count(),
                    Topics = new List<TopicResponse>(x.Topics.Select(a => new TopicResponse
                    {
                        Id = a.Id,
                        Name = a.Name
                    }))
                }).SingleOrDefault(x => x.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found game with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Game By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
