using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IMusicPasswordResourceService
    {
        Task<PagedResults<MusicPasswordResponse>> GetMusicPasswordResources(ResourceRequest request, PagingRequest paging);
        Task<MusicPasswordResponse> CreateMusicPasswordResource(MusicPasswordRequest createMusicPasswordRequest);
        Task<MusicPasswordResponse> GetMusicPasswordResourceById(int id);
        Task<MusicPasswordResponse> UpdateMusicPasswordResource(int id, MusicPasswordRequest request);
        Task<MusicPasswordResponse> DeleteMusicPasswordResource(int id);
    }
}
