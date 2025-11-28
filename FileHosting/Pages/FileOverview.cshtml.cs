using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;                      // IFormFile
using FileHostingBackend.Repos;                       // IStoredFileInfoRepo
using FileHostingBackend.Models;


namespace FileHosting.Pages
{
    public class FileOverviewModel : PageModel
    {
        private readonly IStoredFileInfoRepo _storedFileInfoRepo;

        public FileOverviewModel(IStoredFileInfoRepo fileStorageService)
        {
            _storedFileInfoRepo = fileStorageService;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public List<StoredFileInfo> Files { get; set; } = new();

        public async Task OnGetAsync()
        {
            Files = await _storedFileInfoRepo.GetAllFilesAsync();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Upload == null || Upload.Length == 0)
            {
                ModelState.AddModelError("Upload", "Please select a file");
                return Page();
            }

            await _storedFileInfoRepo.UploadFileAsync(Upload);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDownloadAsync(string fileName)
        {
            var stream = await _storedFileInfoRepo.DownloadFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }
}
