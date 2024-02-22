using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IMusicPasswordResourceOfContestService
    {
        Task<PagedResults<MusicPasswordOfContestResponse>> GetMusicPasswordResourcesOfContest(ResourceOfContestRequest request, PagingRequest paging);
        Task<MusicPasswordOfContestResponse> CreateMusicPasswordResourceOfContest(MusicPasswordOfContestRequest createMusicPasswordOfContestRequest);
        Task<MusicPasswordOfContestResponse> GetMusicPasswordResourceOfContestById(int id);
        Task<MusicPasswordOfContestResponse> UpdateMusicPasswordResourceOfContest(int id, MusicPasswordOfContestRequest request);
        Task<MusicPasswordOfContestResponse> DeleteMusicPasswordResourceOfContest(int id);
    }
}
