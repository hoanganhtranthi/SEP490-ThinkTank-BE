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
    public class AnonymousResourceOfContestService : IAnonymousResourceOfContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public AnonymousResourceOfContestService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<AnonymityOfContestResponse> CreateAnonymityOfContestResource(AnonymityOfContestRequest createAnonymityOfContestRequest)
        {
            try
            {
                var anonymous = _mapper.Map<AnonymityOfContestRequest, AnonymityOfContest>(createAnonymityOfContestRequest);
                var s = _unitOfWork.Repository<AnonymityOfContest>().Find(s => s.Description==createAnonymityOfContestRequest.Description && s.Characteristic==createAnonymityOfContestRequest.Characteristic);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");
                if (createAnonymityOfContestRequest.Characteristic <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Characteristic is invalid", "");

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == anonymous.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {createAnonymityOfContestRequest.ContestId} is not found !!!", "");

                await _unitOfWork.Repository<AnonymityOfContest>().CreateAsync(anonymous);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymityOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymityOfContestVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<AnonymityOfContestResponse>(anonymous);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Anonymity Of Contest Resource Error!!!", ex?.Message);
            }
        }

        public async Task<AnonymityOfContestResponse> DeleteAnonymityOfContestResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<AnonymityOfContest>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<AnonymityOfContest>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymityOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymityOfContestVersion", version += 1, expiryTime);

                var rs = _mapper.Map<AnonymityOfContestResponse>(response);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete Resource Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<AnonymityOfContestResponse> GetAnonymityOfContestResourceById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<AnonymityOfContest>().Find(x => x.Id==id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<AnonymityOfContestResponse>(response);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Resource By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<AnonymityOfContestResponse>> GetAnonymityOfContestResources(ResourceOfContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<AnonymityOfContestResponse>(request);
                var resources = _unitOfWork.Repository<AnonymityOfContest>().GetAll().Include(x => x.Contest)
                    .Select(x => new AnonymityOfContestResponse
                    {
                        Id = x.Id,
                        Characteristic = x.Characteristic,
                        Description = x.Description,
                        LinkImg = x.LinkImg,
                        ContestId = x.ContestId
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<AnonymityOfContestResponse>.Sorting(paging.SortType, resources, paging.ColName);
                var result = PageHelper<AnonymityOfContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<AnonymityOfContestResponse> UpdateAnonymityOfContestResource(int id, AnonymityOfContestRequest request)
        {
            try
            {
                AnonymityOfContest anonymous = _unitOfWork.Repository<AnonymityOfContest>()
                      .Find(c => c.Id == id);

                if (anonymous== null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found anonymous resource of contest with id {id.ToString()}", "");

                if (anonymous.Characteristic <= 0)
                    throw new CrudException(HttpStatusCode.BadRequest, "Characteristic is invalid", "");

                var s = _unitOfWork.Repository<AnonymityOfContest>().Find(s => s.Description == request.Description && s.Characteristic == request.Characteristic && s.Id != anonymous.Id);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == anonymous.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {request.ContestId} is not found !!!", "");
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("AnonymityOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("AnonymityOfContestVersion", version+=1, expiryTime);
                _mapper.Map<AnonymityOfContestRequest, AnonymityOfContest>(request, anonymous);
                await _unitOfWork.Repository<AnonymityOfContest>().Update(anonymous, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<AnonymityOfContestResponse>(anonymous);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update Anonymity of Contest resource error!!!!!", ex.Message);
            }
        }
    }
}
