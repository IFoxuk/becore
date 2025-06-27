using becore.api.Models;

namespace becore.api.Services.Interfaces;

public interface IFileS3Service
{
    Task<FileModel?> CreateAsync(FileModel fileModel);
    Task<FileModel?> GetAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<FileModel?> UpdateAsync(FileModel fileModel);
}
