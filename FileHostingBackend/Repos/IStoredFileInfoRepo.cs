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
        Task<string> UploadFileAsync(IFormFile file);
        Task<List<StoredFileInfo>> GetAllFilesAsync();
        Task<Stream> DownloadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
        Task SoftDeleteAsync (string fileName);


    }
}
