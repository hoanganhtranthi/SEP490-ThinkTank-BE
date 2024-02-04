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
        Task<string> UploadFileProfileAsync(Stream fileStream, string fileName,FileType type);
        Task<string> UploadFileResourceAsync(Stream fileStream, string fileName, ResourceType type,string name);
    }
}
