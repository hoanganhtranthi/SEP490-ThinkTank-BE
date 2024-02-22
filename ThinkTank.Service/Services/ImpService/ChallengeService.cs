using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        public async Task<PagedResults<ChallengeResponse>> GetChallenges(ChallengeRequest request, PagingRequest paging)
        {
            try
            {
                var filter = _mapper.Map<ChallengeResponse>(request);
                var challenges = _unitOfWork.Repository<Challenge>().GetAll()
                                           .ProjectTo<ChallengeResponse>(_mapper.ConfigurationProvider)
                                           .DynamicFilter(filter)
                                           .ToList();
                var sort = PageHelper<ChallengeResponse>.Sorting(paging.SortType, challenges, paging.ColName);
                var result = PageHelper<ChallengeResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get challenge list error!!!!!", ex.Message);
            }
        }
    }
}
