

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.Contests.Queries.GetContestById
{
    public class GetContestByIdQueryHandler : IQueryHandler<GetContestByIdQuery, ContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetContestByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ContestResponse> Handle(GetContestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<Contest>().GetAll().AsNoTracking().Include(x => x.Game).Include(x => x.AccountInContests)
                                           .Select(x => new ContestResponse
                                           {
                                               Id = x.Id,
                                               EndTime = x.EndTime,
                                               StartTime = x.StartTime,
                                               AssetOfContests = _mapper.Map<List<AssetOfContestResponse>>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                                               {
                                                   ContestId = a.ContestId,
                                                   Id = a.Id,
                                                   Value = a.Value,
                                                   NameOfContest = x.Name,
                                                   Answer = x.GameId == 2 ? System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).Substring(0, System.IO.Path.GetFileName(new Uri(a.Value).LocalPath).LastIndexOf('.')) : null,
                                                   Type = a.TypeOfAsset.Type
                                               })),
                                               Name = x.Name,
                                               Status = x.Status,
                                               Thumbnail = x.Thumbnail,
                                               GameId = x.GameId,
                                               PlayTime = x.PlayTime,
                                               CoinBetting = x.CoinBetting,
                                               GameName = x.Game.Name,
                                               AmoutPlayer = x.AccountInContests.Count()
                                           }).SingleOrDefault(u => u.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
