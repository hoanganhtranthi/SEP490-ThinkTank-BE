using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;

namespace ThinkTank.Service.Services.IService
{
    public interface IFlipCardAndImagesWalkthroughResourceService
    {
        Task<PagedResults<FlipCardAndImagesWalkthroughResponse>> GetFlipCardAndImagesWalkthroughResources(ResourceRequest request, PagingRequest paging);
        Task<FlipCardAndImagesWalkthroughResponse> CreateFlipCardAndImagesWalkthroughResource(FlipCardAndImagesWalkthroughRequest createFlipCardAndImagesWalkthroughRequest);
        Task<FlipCardAndImagesWalkthroughResponse> GetFlipCardAndImagesWalkthroughResourceById(int id);
        Task<FlipCardAndImagesWalkthroughResponse> UpdateFlipCardAndImagesWalkthroughResource(int id, FlipCardAndImagesWalkthroughRequest request);
        Task<FlipCardAndImagesWalkthroughResponse> DeleteFlipCardAndImagesWalkthroughResource(int id);
    }
}
