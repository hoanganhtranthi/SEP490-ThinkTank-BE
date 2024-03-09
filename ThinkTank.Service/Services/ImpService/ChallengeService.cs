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
    public class ChallengeService : IChallengeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public ChallengeService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = configuration;
        }
        public async Task<List<ChallengeResponse>> GetChallenges(ChallengeRequest request)
        {
            try
            {
                var challenges = _unitOfWork.Repository<Challenge>().GetAll().Include(x=>x.Badges)
                                           .Select(x=>new ChallengeResponse
                                           {
                                               Id=x.Id,
                                               Avatar=x.Avatar,
                                               CompletedMilestone=x.CompletedMilestone,
                                               Description=x.Description,
                                               MissionsImg=x.MissionsImg,
                                               Name = x.Name,
                                               Unit=x.Unit,
                                               CompletedLevel= x.Badges.SingleOrDefault(a=>a.ChallengeId==x.Id && a.AccountId==request.AccountId).CompletedLevel,
                                               CompletedDate= x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).CompletedDate,
                                               Status= x.Badges.SingleOrDefault(a => a.ChallengeId == x.Id && a.AccountId == request.AccountId).Status
                                           })
                                           .ToList();
                if (request.Status != Helpers.Enum.StatusType.All)
                {
                    bool? status = null;
                    if (request.Status.ToString().ToLower() != "null")
                    {
                        status = bool.Parse(request.Status.ToString().ToLower());
                    }
                    challenges = challenges.Where(x => x.Status.Equals(status)).ToList();
                }
                return challenges;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get challenge list error!!!!!", ex.Message);
            }
        }
    }
}
