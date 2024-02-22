using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IPrizeOfContestService
    {
        Task<PagedResults<PrizeOfContestResponse>> GetPrizeOfContests(ResourceOfContestRequest request, PagingRequest paging);
        Task<PrizeOfContestResponse> CreatePrizeOfContest(CreatePrizeOfContestRequest createPrizeOfContestRequest);
    }
}
