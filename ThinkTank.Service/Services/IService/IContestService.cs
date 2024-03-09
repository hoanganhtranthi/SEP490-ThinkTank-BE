using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IContestService
    {
        Task<PagedResults<ContestResponse>> GetContests(ContestRequest request, PagingRequest paging);
        Task<ContestResponse> CreateContest(CreateAndUpdateContestRequest createContestRequest);
        Task<ContestResponse> GetContestById(int id);
        Task<ContestResponse> UpdateContest(int contestId, CreateAndUpdateContestRequest request);
        Task<ContestResponse> UpdateStateContest(int id);
        Task<ContestResponse> DeleteContest(int id);
        Task<List<LeaderboardResponse>> GetLeaderboardOfContest(int id);
        Task<dynamic> GetReportOfContest();
    }
}
