using FileHostingBackend.Repos;
using FileHostingBackend.Models;

namespace FileHostingBackend.Services
{
    public class StoredFileInfoService
    {
        private readonly IStoredFileInfoRepo _storedFileInfoRepo;

        public StoredFileInfoService(IStoredFileInfoRepo storedFileInfoRepo)
        {
            _storedFileInfoRepo = storedFileInfoRepo;
        }
        public async Task<string> UploadFileAsync(Microsoft.AspNetCore.Http.IFormFile file, User user)
        {
            await _storedFileInfoRepo.UploadFileAsync(file, user);
            return "File uploaded successfully";

        }
        public async Task<List<Models.StoredFileInfo>> GetAllFilesAsync()
        {
            return await _storedFileInfoRepo.GetAllFilesAsync();
        }


        public async Task SoftDeleteAsync(string fileName)
        {
            await _storedFileInfoRepo.SoftDeleteAsync(fileName);
        }

        // New wrappers for wastebasket

        public async Task<List<StoredFileInfo>> GetFilesWithPermissionAsync(int userId)
        {             
            return await _storedFileInfoRepo.GetFilesWithPermissionAsync(userId);
        }

        #region Download Function
        // Prototype wrapper: get presigned download URL from the repo

        // New: wrapper for server-side streaming download
        public async Task<(System.IO.Stream Stream, string ContentType)> GetObjectWithContentTypeAsync(string filePath)
        {
            return await _storedFileInfoRepo.GetObjectWithContentTypeAsync(filePath);
        }
        #endregion
    }
}
