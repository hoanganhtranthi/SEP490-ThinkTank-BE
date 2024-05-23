
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.Services.IService
{
    public interface IContestService
    {
        Task<PagedResults<ContestResponse>> GetContests(ContestRequest request, PagingRequest paging);
        Task<ContestResponse> CreateContest(CreateAndUpdateContestRequest createContestRequest);
        Task<ContestResponse> GetContestById(int id);
        Task<ContestResponse> UpdateContest(int contestId, CreateAndUpdateContestRequest request);
        Task<ContestResponse> DeleteContest(int id);
        Task<PagedResults<LeaderboardResponse>> GetLeaderboardOfContest(int contestId, PagingRequest paging);
        Task<dynamic> GetReportOfContest();
    }
}
