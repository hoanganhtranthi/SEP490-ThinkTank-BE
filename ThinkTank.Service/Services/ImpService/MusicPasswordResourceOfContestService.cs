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
    public class MusicPasswordResourceOfContestService : IMusicPasswordResourceOfContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public MusicPasswordResourceOfContestService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<MusicPasswordOfContestResponse> CreateMusicPasswordResourceOfContest(MusicPasswordOfContestRequest createMusicPasswordOfContestRequest)
        {
            try
            {
                var musicPassword = _mapper.Map<MusicPasswordOfContestRequest, MusicPasswordOfContest>(createMusicPasswordOfContestRequest);
                var s = _unitOfWork.Repository<MusicPasswordOfContest>().Find(s => s.Password == createMusicPasswordOfContestRequest.Password);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == musicPassword.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {createMusicPasswordOfContestRequest.ContestId} is not found !!!", "");

                await _unitOfWork.Repository<MusicPasswordOfContest>().CreateAsync(musicPassword);
                await _unitOfWork.CommitAsync();
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordOfContestVersion", version += 1, expiryTime);
                else version = 1;
                var rs = _mapper.Map<MusicPasswordOfContestResponse>(musicPassword);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Add Music Password Resource Of Contest Error!!!", ex?.Message);
            }
        }

        public async Task<MusicPasswordOfContestResponse> DeleteMusicPasswordResourceOfContest(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<MusicPasswordOfContest>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource with id {id.ToString()}", "");
                }
                _unitOfWork.Repository<MusicPasswordOfContest>().Delete(response);
                await _unitOfWork.CommitAsync();

                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordOfContestVersion", version += 1, expiryTime);

                var rs = _mapper.Map<MusicPasswordOfContestResponse>(response);
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

        public async Task<MusicPasswordOfContestResponse> GetMusicPasswordResourceOfContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Resource Invalid", "");
                }
                var response = _unitOfWork.Repository<MusicPasswordOfContest>().Find(x => x.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found resource of contest with id {id.ToString()}", "");
                }
                var rs = _mapper.Map<MusicPasswordOfContestResponse>(response);
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

        public async Task<PagedResults<MusicPasswordOfContestResponse>> GetMusicPasswordResourcesOfContest(ResourceOfContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<MusicPasswordOfContestResponse>(request);
                var resources = _unitOfWork.Repository<MusicPasswordOfContest>().GetAll().Include(x => x.Contest)
                    .Select(x => new MusicPasswordOfContestResponse
                    {
                        Id = x.Id,
                        Password=x.Password,
                        ContestId = x.ContestId
                    }).DynamicFilter(filter).ToList();
                var sort = PageHelper<MusicPasswordOfContestResponse>.Sorting(paging.SortType, resources, paging.ColName);
                var result = PageHelper<MusicPasswordOfContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get resources list error!!!!!", ex.Message);
            }
        }

        public async Task<MusicPasswordOfContestResponse> UpdateMusicPasswordResourceOfContest(int id, MusicPasswordOfContestRequest request)
        {
            try
            {
                MusicPasswordOfContest musicPassword = _unitOfWork.Repository<MusicPasswordOfContest>()
                      .Find(c => c.Id == id);

                if (musicPassword == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found Music Password resource of Contest with id {id.ToString()}", "");

                var s = _unitOfWork.Repository<MusicPasswordOfContest>().Find(s => s.Password == request.Password && s.Id != id);
                if (s != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "This resource has already !!!", "");

                var contest = _unitOfWork.Repository<Contest>().Find(x => x.Id == musicPassword.ContestId);
                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"This contest {request.ContestId} is not found !!!", "");
                var expiryTime = DateTime.MaxValue;
                var version = _cacheService.GetData<int>("MusicPasswordOfContestVersion");
                if (version != null)
                    _cacheService.SetData<int>("MusicPasswordOfContestVersion", version += 1, expiryTime);
                _mapper.Map<MusicPasswordOfContestRequest, MusicPasswordOfContest>(request, musicPassword);
                await _unitOfWork.Repository<MusicPasswordOfContest>().Update(musicPassword, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<MusicPasswordOfContestResponse>(musicPassword);
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update music password resource of contest error!!!!!", ex.Message);
            }
        }
    }
}
