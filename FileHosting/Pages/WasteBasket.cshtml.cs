using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FileHostingBackend.Repos;
using FileHostingBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace FileHosting.Pages
{
    [Authorize]
    public class WastebasketModel : PageModel
    {
        private readonly IStoredFileInfoRepo _storedFileInfoRepo;

        public WastebasketModel(IStoredFileInfoRepo storedFileInfoRepo) // Inject the repository for stored file info
        {
            _storedFileInfoRepo = storedFileInfoRepo;
        }

        public List<StoredFileInfo> DeletedFiles { get; set; } = new();

        public async Task OnGetAsync()
        {
            DeletedFiles = await _storedFileInfoRepo.GetDeletedFilesAsync();
        }

        public async Task<IActionResult> OnPostRestoreAsync([FromForm] string filePath) // Handle restoration of a soft-deleted file
        {
            if (string.IsNullOrEmpty(filePath)) return BadRequest(); // Validate file path

            await _storedFileInfoRepo.RestoreAsync(filePath);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromForm] string filePath) // Handle permanent deletion of a file
        {
            if (string.IsNullOrEmpty(filePath)) return BadRequest(); // Validate file path

            await _storedFileInfoRepo.DeleteFileAsync(filePath);
            return RedirectToPage();
        }
    }
}
