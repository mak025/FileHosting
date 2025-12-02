using FileHostingBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileHostingBackend.Repos
{
    public interface IStoredFileInfoRepo
    {
        Task<string> UploadFileAsync(IFormFile file, User user);
        Task<List<StoredFileInfo>> GetAllFilesAsync();
        Task DeleteFileAsync(string fileName);
        Task SoftDeleteAsync(string fileName);

        // Required members for wastebasket/restore
        Task<List<StoredFileInfo>> GetDeletedFilesAsync();
        Task RestoreAsync(string fileName);
        //Task PermanentlyDeleteAsync(string fileName);
    }
}
