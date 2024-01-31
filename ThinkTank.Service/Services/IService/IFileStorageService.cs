using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ThinkTank.Service.Helpers.Enum;

namespace ThinkTank.Service.Services.IService
{
    public interface IFileStorageService
    {
        Task<string> UploadFileToDefaultAsync(Stream fileStream, string fileName);
        Task<string> UploadFileResourceAsync(Stream fileStream, string fileName, ResourceType type);
    }
}
