using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/versionOfResources")]
    [ApiController]
    public class VersionResourcesController : Controller
    {
        private readonly IVersionOfResourceService _versionOfResourceService;
        public VersionResourcesController(IVersionOfResourceService versionOfResourceService)
        {
            _versionOfResourceService = versionOfResourceService;
        }
        /// <summary>
        /// Get current version of resources
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<string>> GetVersionOfResources()
        {
            var rs = await _versionOfResourceService.GetCurrentVersionOfResource();
            return Ok(rs);
        }
    }
}
