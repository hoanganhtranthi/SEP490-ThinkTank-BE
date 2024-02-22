using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public class BadgeService : IBadgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public BadgeService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
        }

        public async Task<BadgeResponse> CreateBadge(CreateBadgeRequest createBadgeRequest)
        {
            try
            {
                var badge = _mapper.Map<CreateBadgeRequest, Badge>(createBadgeRequest);
                var s = _unitOfWork.Repository<Badge>().Find(s => s.ChallengeId == createBadgeRequest.ChallengeId && s.AccountId == createBadgeRequest.AccountId);
                if (s != null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Badge has already !!!", "");
                }

                var a = _unitOfWork.Repository<Account>().Find(a => a.Id == createBadgeRequest.AccountId);
                if (a == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account Not Found!!!!!", "");
                }
                badge.AccountId = createBadgeRequest.AccountId;

                var c = _unitOfWork.Repository<Challenge>().Find(c => c.Id == createBadgeRequest.ChallengeId);
                if (c == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Challenge Not Found!!!!!", "");
                }
                badge.ChallengeId = createBadgeRequest.ChallengeId;
                badge.CompletedLevel = createBadgeRequest.CompletedLevel;
                badge.Status = false;

                await _unitOfWork.Repository<Badge>().CreateAsync(badge);
                await _unitOfWork.CommitAsync();

                return _mapper.Map<BadgeResponse>(badge);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Create Badge Error!!!", ex?.Message);
            }
        }

        public async Task<PagedResults<BadgeResponse>> GetBadges(BadgeRequest request, PagingRequest paging)
        {
            try
            {
                var acc = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId);
                if (acc == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account Not Found!!!!!", "");
                }
                else
                {
                    var filter = _mapper.Map<BadgeResponse>(request);
                    var badges = _unitOfWork.Repository<Badge>().GetAll().Include(a => a.Challenge).Include(a => a.AccountId == request.AccountId)
                        .Select(x => new BadgeResponse
                        {
                            Id = x.Id,
                            Name = x.Challenge.Name,
                            Avatar = x.Challenge.Avatar,
                            Description = x.Challenge.Description,
                            CompletedLevel = x.CompletedLevel,
                            CompletedMilestone = x.Challenge.CompletedMilestone,
                            Status = x.Status
                        }).DynamicFilter(filter).ToList();
                    var sort = PageHelper<BadgeResponse>.Sorting(paging.SortType, badges, paging.ColName);
                    var result = PageHelper<BadgeResponse>.Paging(sort, paging.Page, paging.PageSize);
                    return result;
                }
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get badge list error!!!!!", ex.Message);
            }
        }

        public async Task<PagedResults<BadgeResponse>> GetBadgesIsCompleted(BadgeRequest request, PagingRequest paging)
        {
            try
            {
                var acc = _unitOfWork.Repository<Account>().Find(a => a.Id == request.AccountId);
                if (acc == null)
                {
                    throw new CrudException(HttpStatusCode.InternalServerError, "Account Not Found!!!!!", "");
                }
                else
                {
                    var filter = _mapper.Map<BadgeResponse>(request);
                    var badges = _unitOfWork.Repository<Badge>().GetAll().Include(a => a.Challenge).Include(a => a.AccountId == request.AccountId).Include(a => a.Status.Equals(true))
                        .Select(x => new BadgeResponse
                        {
                            Id = x.Id,
                            Name = x.Challenge.Name,
                            Avatar = x.Challenge.Avatar,
                            Description = x.Challenge.Description,
                            CompletedLevel = x.CompletedLevel,
                            CompletedMilestone = x.Challenge.CompletedMilestone,
                        }).DynamicFilter(filter).ToList();
                    var sort = PageHelper<BadgeResponse>.Sorting(paging.SortType, badges, paging.ColName);
                    var result = PageHelper<BadgeResponse>.Paging(sort, paging.Page, paging.PageSize);
                    return result;
                }
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get badge list error!!!!!", ex.Message);
            }
        }

        public async Task<BadgeResponse> UpdateBadge(int badgeId, CreateBadgeRequest request)
        {
            try
            {
                Badge badge = _unitOfWork.Repository<Badge>()
                     .Find(c => c.Id == badgeId);

                if (badge == null)
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found badge with id {badgeId.ToString()}", "");

                _mapper.Map<CreateBadgeRequest, Badge>(request, badge);

                await _unitOfWork.Repository<Badge>().Update(badge, badgeId);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<Badge, BadgeResponse>(badge);
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update badge error!!!!!", ex.Message);
            }
        }
    }
}
