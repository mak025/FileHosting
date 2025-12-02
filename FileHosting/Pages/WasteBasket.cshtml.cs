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

        public WastebasketModel(IStoredFileInfoRepo storedFileInfoRepo)
        {
            _storedFileInfoRepo = storedFileInfoRepo;
        }

        public List<StoredFileInfo> DeletedFiles { get; set; } = new();

        public async Task OnGetAsync()
        {
            DeletedFiles = await _storedFileInfoRepo.GetDeletedFilesAsync();
        }

        public async Task<IActionResult> OnPostRestoreAsync([FromForm] string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return BadRequest();

            await _storedFileInfoRepo.RestoreAsync(filePath);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromForm] string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return BadRequest();

            await _storedFileInfoRepo.PermanentlyDeleteAsync(filePath);
            return RedirectToPage();
        }
    }
}
