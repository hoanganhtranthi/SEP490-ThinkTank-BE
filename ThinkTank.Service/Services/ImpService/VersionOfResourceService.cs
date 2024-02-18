using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.Service.Services.ImpService
{
    public class VersionOfResourceService : IVersionOfResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        public VersionOfResourceService(IUnitOfWork unitOfWork,ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<string> GetCurrentVersionOfResource()
        {
            try
            {
                var sb = new StringBuilder();

                var anonymousVersion = _cacheService.GetData<int>("AnonymousVersion");
                if (anonymousVersion != null)
                {
                    sb.Append($"anonymousVersion{anonymousVersion} ");
                }

                var flipCardAndImagesWalkthroughVersion = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughVersion");
                if (flipCardAndImagesWalkthroughVersion != null)
                {
                    sb.Append($"flipCardAndImagesWalkthroughVersion{flipCardAndImagesWalkthroughVersion} ");
                }

                var musicPasswordVersion = _cacheService.GetData<int>("MusicPasswordVersion");
                if (musicPasswordVersion != null)
                {
                    sb.Append($"musicPasswordVersion{musicPasswordVersion}  ");
                }

                var storyTellerVersion = _cacheService.GetData<int>("StoryTellerVersion");
                if (storyTellerVersion != null)
                {
                    sb.Append($"storyTellerVersion{storyTellerVersion}");
                }

                return sb.ToString();
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get version of resource error!!!", ex.InnerException?.Message);
            }
        }

    }
}
