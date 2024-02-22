using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class ContestService : IContestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public ContestService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task<ContestResponse> CreateContest(CreateContestRequest createContestRequest)
        {
            try
            {
                var contest = _mapper.Map<CreateContestRequest, Contest>(createContestRequest);
                var s = _unitOfWork.Repository<Contest>().Find(s => s.Name == createContestRequest.Name);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Contest has already !!!", "");
                }

                contest.Name = createContestRequest.Name;
                contest.Thumbnail = createContestRequest.Thumbnail;
                contest.StartTime = createContestRequest.StartTime;
                contest.EndTime = createContestRequest.EndTime;
                if (contest.StartTime > contest.EndTime)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");
                }
                contest.Status = true;
                //create prize of contest
                await _unitOfWork.Repository<Contest>().CreateAsync(contest);
                await _unitOfWork.CommitAsync();
                
                return _mapper.Map<ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Contest Error!!!", ex?.Message);
            }
        }

        public async Task<ContestResponse> UpdateStateContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>()
                      .Find(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                contest.Status = false;
                await _unitOfWork.Repository<Contest>().Update(contest, id);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status's contest error!!!!!", ex.Message);
            }
        }

        public async Task<ContestResponse> GetContestById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Contest Invalid", "");
                }
                var response = await _unitOfWork.Repository<Contest>().GetAsync(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {id.ToString()}", "");
                }

                return _mapper.Map<ContestResponse>(response);
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

        public async Task<PagedResults<ContestResponse>> GetContests(CreateContestRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ContestResponse>(request);
                var contests = _unitOfWork.Repository<Contest>().GetAll()
                                           .ProjectTo<ContestResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                var sort = PageHelper<ContestResponse>.Sorting(paging.SortType, contests, paging.ColName);
                var result = PageHelper<ContestResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get contest list error!!!!!", ex.Message);
            }
        }

        public async Task<ContestResponse> UpdateContest(int contestId, CreateContestRequest request)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>()
                     .Find(c => c.Id == contestId);

                if (contest == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id {contestId.ToString()}", "");

                var existingContest = _unitOfWork.Repository<Contest>().GetAll().FirstOrDefault(c => c.Name.Equals(request.Name) && c.Id != contestId);
                if (existingContest != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Name of contest has already been taken", "");

                _mapper.Map<CreateContestRequest, Contest>(request, contest);

                contest.Id = contestId;
                if (request.StartTime > request.EndTime)
                    throw new CrudException(HttpStatusCode.BadRequest, "Start Time or End Time is invalid", "");

                await _unitOfWork.Repository<Contest>().Update(contest, contestId);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Contest, ContestResponse>(contest);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update contest error!!!!!", ex.Message);
            }
        }

        public async Task<dynamic> DeleteContest(int id)
        {
            try
            {
                Contest contest = _unitOfWork.Repository<Contest>()
                      .Find(c => c.Id == id);

                if (contest == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with id{id.ToString()}", "");
                }
                await _unitOfWork.Repository<Contest>().RemoveAsync(contest);
                await _unitOfWork.CommitAsync();
                return new CrudException(HttpStatusCode.OK, "Delete contest success", "");
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete contest error!!!!!", ex.Message);
            }
        }
    }
}

/*public async Task<ContestResponse> GetContestByName(string name)
{
    try
    {
        if (name == null)
        {
            throw new CrudException(HttpStatusCode.BadRequest, "Name Contest Invalid", "");
        }
        var response = await _unitOfWork.Repository<Contest>().GetAsync(u => u.Name.Equals(name));

        if (response == null)
        {
            throw new CrudException(HttpStatusCode.NotFound, $"Not found contest with name {name.ToString()}", "");
        }

        return _mapper.Map<ContestResponse>(response);
    }
    catch (CrudException ex)
    {
        throw ex;
    }
    catch (Exception ex)
    {
        throw new CrudException(HttpStatusCode.InternalServerError, "Get Contest By Name Error!!!", ex.InnerException?.Message);
    }
}
*/

