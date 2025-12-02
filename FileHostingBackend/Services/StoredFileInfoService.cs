using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        
        public async Task DeleteFileAsync(string fileName)
        {
            await _storedFileInfoRepo.DeleteFileAsync(fileName);
        }
        public async Task SoftDeleteAsync(string fileName)
        {
            await _storedFileInfoRepo.SoftDeleteAsync(fileName);
        }

        // New wrappers for wastebasket
        public async Task<List<StoredFileInfo>> GetDeletedFilesAsync()
        {
            return await _storedFileInfoRepo.GetDeletedFilesAsync();
        }

        public async Task RestoreAsync(string fileName)
        {
            await _storedFileInfoRepo.RestoreAsync(fileName);
        }

        public async Task PermanentlyDeleteAsync(string fileName)
        {
            await _storedFileInfoRepo.PermanentlyDeleteAsync(fileName);
        }
    }
}
