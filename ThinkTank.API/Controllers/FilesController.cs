
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using ThinkTank.Service.Services.IService;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        public const long MAX_UPLOAD_FILE_SIZE = 25000000;//File size must lower than 25MB
        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        /// <summary>
        /// Upload file (FileType: System=1, Player=2)
        /// </summary>
        /// <param name="type"></param>
        [HttpPost]
        public async Task<ActionResult<string>> UploadFile(IFormFile file,FileType type)
        {
            if (file.Length > MAX_UPLOAD_FILE_SIZE)
                return BadRequest("Exceed 25MB");
            string url = await _fileStorageService.UploadFileAsync(file.OpenReadStream(), file.FileName,type);
            return Ok(url);
        }
        /// <summary>
        /// Upload file for resources game ( ResourceType: Anonymous=1,MusicPassword=2,FlipCard=3,ImagesWalkthrough=4)
        /// </summary>
        [HttpPost("resources")]
        public async Task<ActionResult<List<string>>> UploadFileResource(List<IFormFile> file,ResourceType type)
        {
            var list = new List<string>();
            foreach (var fileItem in file)
            {
                if (type == ResourceType.MusicPassword && Path.GetExtension(fileItem.FileName).ToLowerInvariant() != ".mp3")
                    return BadRequest("Invalid Extension Of File");
                if (fileItem.Length > MAX_UPLOAD_FILE_SIZE)
                    return BadRequest("Exceed 25MB");
                string url = await _fileStorageService.UploadFileResourceAsync(fileItem.OpenReadStream(), fileItem.FileName, type, "Resources");
                list.Add(url);
            }
            return list;

        }
        /// <summary>
        /// Upload file for contest resources game ( ResourceType: Anonymous=1,MusicPassword=2,FlipCard=3,ImagesWalkthrough=4)
        /// </summary>
        [HttpPost("contests")]
        public async Task<ActionResult<List<string>>> UploadFileContestResource(List<IFormFile> file, ResourceType type)
        {
            var list = new List<string>();
            foreach (var fileItem in file)
            {
                if (type == ResourceType.MusicPassword && Path.GetExtension(fileItem.FileName).ToLowerInvariant() != ".mp3")
                    return BadRequest("Invalid Extension Of File");
                if (fileItem.Length > MAX_UPLOAD_FILE_SIZE)
                    return BadRequest("Exceed 25MB");
                string url = await _fileStorageService.UploadFileResourceAsync(fileItem.OpenReadStream(), fileItem.FileName, type, "Contest");
                list.Add(url);
            }
            return list;
        }
    }
}
