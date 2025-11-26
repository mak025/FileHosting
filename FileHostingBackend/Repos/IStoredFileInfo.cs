using FileHostingBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FileHostingBackend.Repos
{
    public interface IStoredFileInfo
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<List<StoredFileInfo>> GetAllFilesAsync();
        Task<Stream> DownloadFileAsync(string fileName);

    }
}
