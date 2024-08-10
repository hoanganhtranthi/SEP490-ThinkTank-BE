
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.Configuration.Queries;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.CQRS.TypeOfAssetInContests.Queries.GetTypeOfAssetInContestById
{
    public class GetTypeOfAssetsInContestByIdQueryHandler : IQueryHandler<GetTypeOfAssetsInContestByIdQuery, TypeOfAssetInContestResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlackService _slackService;
        public GetTypeOfAssetsInContestByIdQueryHandler(IUnitOfWork unitOfWork, ISlackService slackService)
        {
            _unitOfWork = unitOfWork;
            _slackService = slackService;
        }

        public async Task<TypeOfAssetInContestResponse> Handle(GetTypeOfAssetsInContestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = _unitOfWork.Repository<TypeOfAssetInContest>().GetAll().AsNoTracking().Include(x => x.AssetOfContests).Select(x => new TypeOfAssetInContestResponse
                {
                    Id = x.Id,
                    Type = x.Type,
                    AssetOfContests = new List<AssetOfContestResponse>(x.AssetOfContests.Select(a => new AssetOfContestResponse
                    {
                        Id = a.Id,
                        ContestId = a.ContestId,
                        NameOfContest = a.Contest.Name,
                        Type = a.TypeOfAsset.Type,
                        Value = a.Value
                    }))
                }).SingleOrDefault(x => x.Id == request.Id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset in contest with id {request.Id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                await _slackService.SendMessage(_slackService.CreateMessage(ex, "Get type of asset in contest by id error!!!"));
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of asset in contest by id error!!!", ex.InnerException?.Message);
            }
        }
    }
}
