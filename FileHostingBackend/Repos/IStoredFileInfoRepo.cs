using FileHostingBackend.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FileHostingBackend.Repos
{
    public interface IStoredFileInfoRepo
    {
        Task<string> UploadFileAsync(IFormFile file, User user);
        Task<List<StoredFileInfo>> GetAllFilesAsync();
        Task DeleteFileAsync(string fileName);
        Task SoftDeleteAsync (string fileName);
        Task<string> GetPresignedDownloadUrlAsync(string filePath, string downloadFileName, TimeSpan? expiry = null);
    }
}
