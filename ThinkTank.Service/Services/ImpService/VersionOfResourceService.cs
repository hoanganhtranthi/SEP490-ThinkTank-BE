﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<dynamic> GetCurrentVersionOfResource()
        {
            try
            {
                return new
                {
                    AnonymousVersion = _cacheService.GetData<int>("AnonymousVersion"),
                    FlipCardAndImagesWalkthroughVersion = _cacheService.GetData<int>("FlipCardAndImagesWalkthroughVersion"),
                    MusicPasswordVersion = _cacheService.GetData<int>("MusicPasswordVersion"),
                    StoryTellerVersion = _cacheService.GetData<int>("StoryTellerVersion")
                };
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
