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
        Task<List<StoredFileInfo>> GetFilesWithPermissionAsync(int userId);
        Task DeleteFileAsync(string fileName);
        Task SoftDeleteAsync(string fileName);

        // Required members for wastebasket/restore
        Task<List<StoredFileInfo>> GetDeletedFilesAsync();
        Task RestoreAsync(string fileName); //Task PermanentlyDeleteAsync(string fileName);
        Task UpdateUserPermissionsAsync(int fileId, List<int> userIds);

        #region Download Function
        // Prototype: get a presigned URL for direct download from storage
        Task<string> GetPresignedUrlAsync(string filePath, TimeSpan? expiry = null);

        // New: retrieve object as a stream plus content-type (for server-side streaming download)
        Task<(System.IO.Stream Stream, string ContentType)> GetObjectWithContentTypeAsync(string filePath);

        #endregion
    }
}
