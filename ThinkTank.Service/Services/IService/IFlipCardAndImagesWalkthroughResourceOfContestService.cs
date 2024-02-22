using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IFlipCardAndImagesWalkthroughResourceOfContestService
    {
        Task<PagedResults<FlipCardAndImagesWalkthroughOfContestResponse>> GetFlipCardAndImagesWalkthroughResourcesOfContest(ResourceOfContestRequest request, PagingRequest paging);
        Task<FlipCardAndImagesWalkthroughOfContestResponse> CreateFlipCardAndImagesWalkthroughResourceOfContest(FlipCardAndImagesWalkthroughOfContestRequest createFlipCardAndImagesWalkthroughOfContestRequest);
        Task<FlipCardAndImagesWalkthroughOfContestResponse> GetFlipCardAndImagesWalkthroughResourceOfContestById(int id);
        Task<FlipCardAndImagesWalkthroughOfContestResponse> UpdateFlipCardAndImagesWalkthroughResourceOfContest(int id, FlipCardAndImagesWalkthroughOfContestRequest request);
        Task<FlipCardAndImagesWalkthroughOfContestResponse> DeleteFlipCardAndImagesWalkthroughResourceOfContest(int id);
    }
}
