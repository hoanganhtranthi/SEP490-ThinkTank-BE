﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IAnonymousResourceOfContestService
    {
        Task<PagedResults<AnonymityOfContestResponse>> GetAnonymityOfContestResources(ResourceOfContestRequest request, PagingRequest paging);
        Task<AnonymityOfContestResponse> CreateAnonymityOfContestResource(AnonymityOfContestRequest createAnonymityOfContestRequest);
        Task<AnonymityOfContestResponse> GetAnonymityOfContestResourceById(int id);
        Task<AnonymityOfContestResponse> UpdateAnonymityOfContestResource(int id, AnonymityOfContestRequest request);
        Task<AnonymityOfContestResponse> DeleteAnonymityOfContestResource(int id);
    }
}