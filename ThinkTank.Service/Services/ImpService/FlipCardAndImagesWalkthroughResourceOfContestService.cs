using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
    public class FlipCardAndImagesWalkthroughResourceOfContestService : IFlipCardAndImagesWalkthroughResourceOfContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public FlipCardAndImagesWalkthroughResourceOfContestService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<FlipCardAndImagesWalkthroughOfContestResponse> CreateFlipCardAndImagesWalkthroughResourceOfContest(FlipCardAndImagesWalkthroughOfContestRequest createFlipCardAndImagesWalkthroughOfContestRequest)
        {
            try
            {
                var flipCardAndImagesWalkthrough = _mapper.Map<FlipCardAndImagesWalkthroughOfContestRequest, FlipCardAndImagesWalkthroughOfContest>(createFlipCardAndImagesWalkthroughOfContestRequest);

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == flipCardAndImagesWalkthrough.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {createFlipCardAndImagesWalkthroughOfContestRequest.ContestId} is not found !!!", "");

                await _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().CreateAsync(flipCardAndImagesWalkthrough);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughOfContestVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughOfContestResponse>(flipCardAndImagesWalkthrough);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add FlipCard And ImagesWalkthrough  Resource Of Contest Error!!!", ex?.Message);
            }
        }

        public async Task<FlipCardAndImagesWalkthroughOfContestResponse> DeleteFlipCardAndImagesWalkthroughResourceOfContest(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource of contest with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughOfContestVersion", version += 1, expiryTime);

                var rs = _mapper.Map<FlipCardAndImagesWalkthroughOfContestResponse>(response);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Resource Of Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<FlipCardAndImagesWalkthroughOfContestResponse> GetFlipCardAndImagesWalkthroughResourceOfContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource of contest with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughOfContestResponse>(response);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Resource Of Contest By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<FlipCardAndImagesWalkthroughOfContestResponse>> GetFlipCardAndImagesWalkthroughResourcesOfContest(ResourceOfContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<FlipCardAndImagesWalkthroughOfContestResponse>(request);
                var resources = _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().GetAll().Include(x => x.Contest)
                    .Select(x => new FlipCardAndImagesWalkthroughOfContestResponse
                    {
                        Id = x.Id,
                        LinkImg=x.LinkImg,
                        ContestId = x.ContestId
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<FlipCardAndImagesWalkthroughOfContestResponse>.Sorting(paging.SortType, resources, paging.ColName);
                var result = PageHelper<FlipCardAndImagesWalkthroughOfContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<FlipCardAndImagesWalkthroughOfContestResponse> UpdateFlipCardAndImagesWalkthroughResourceOfContest(int id, FlipCardAndImagesWalkthroughOfContestRequest request)
        {
            try
            {
                FlipCardAndImagesWalkthroughOfContest flipCardAndImagesWalkthroughOfContest = _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>()
                      .Find(c => c.Id == id);

                if (flipCardAndImagesWalkthroughOfContest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found flipCardAndImagesWalkthrough of contest resource with id {id.ToString()}", "");

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == flipCardAndImagesWalkthroughOfContest.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {request.ContestId} is not found !!!", "");

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("FlipCardAndImagesWalkthroughOfContestVersion", version += 1, expiryTime);

                _mapper.Map<FlipCardAndImagesWalkthroughOfContestRequest, FlipCardAndImagesWalkthroughOfContest>(request, flipCardAndImagesWalkthroughOfContest);
                await _unitOfWork.Repository<FlipCardAndImagesWalkthroughOfContest>().Update(flipCardAndImagesWalkthroughOfContest, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<FlipCardAndImagesWalkthroughOfContestResponse>(flipCardAndImagesWalkthroughOfContest);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update FlipCard And ImagesWalkthrough resource of Contest error!!!!!", ex.Message);
            }
        }
    }
}
