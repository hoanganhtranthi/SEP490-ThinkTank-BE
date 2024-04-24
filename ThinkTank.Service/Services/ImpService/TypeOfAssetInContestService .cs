using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Net;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
{
    public class TypeOfAssetInContestService : ITypeOfAssetInContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TypeOfAssetInContestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResults<TypeOfAssetInContestResponse>> GetTypeOfAssetInContests(TypeOfAssetInContestRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<TypeOfAssetInContestResponse>(request);

                var typeOfAssetResponses = _unitOfWork.Repository<TypeOfAssetInContest>().GetAll().AsNoTracking().Include(x => x.AssetOfContests)
                    .Select(x => new TypeOfAssetInContestResponse
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
                    }).DynamicFilter(filter).ToList();

                if (request.ContestId != null)
                {
                    typeOfAssetResponses = typeOfAssetResponses
                        .Where(asset => asset.AssetOfContests.Any(a => a.ContestId == request.ContestId))
                        .ToList();
                }
                var sort = PageHelper<TypeOfAssetInContestResponse>.Sorting(paging.SortType, typeOfAssetResponses, paging.ColName);
                var result = PageHelper<TypeOfAssetInContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of assets in contest list error!!!!!", ex.Message);
            }
        }
      
       public async Task<TypeOfAssetInContestResponse> GetTypeOfAssetInContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Type Of Asset In Contest Invalid", "");
                }
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
                }).SingleOrDefault(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found type of asset in contest with id {id}", "");
                }
                return response;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get type of asset in contest by id error!!!", ex.InnerException?.Message);
            }
        }
    }
}
