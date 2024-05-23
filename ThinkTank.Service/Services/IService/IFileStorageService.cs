

using static ThinkTank.Domain.Enums.Enum;

namespace ThinkTank.Application.Services.IService
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName,FileType type);
        Task<string> UploadFileResourceAsync(Stream fileStream, string fileName, ResourceType type,string name);
    }
}
