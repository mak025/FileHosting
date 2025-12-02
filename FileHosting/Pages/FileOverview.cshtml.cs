using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;                      // IFormFile
using FileHostingBackend.Repos;                       // IStoredFileInfoRepo
using FileHostingBackend.Models;
using Microsoft.AspNetCore.Authorization;


namespace FileHosting.Pages
{
    [Authorize]
    public class FileOverviewModel : PageModel
    {
        private readonly IStoredFileInfoRepo _storedFileInfoRepo;
        private readonly FileHostDBContext _dbContext;

        public FileOverviewModel(IStoredFileInfoRepo fileStorageService, FileHostDBContext dbContext)
        {
            _storedFileInfoRepo = fileStorageService;
            _dbContext = dbContext;
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

            // Get user ID from claims
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ModelState.AddModelError("", "User not authenticated.");
                return Page();
            }

            // Fetch user from DB
            var user = await _dbContext.Users.FindAsync(userId); // _dbContext injected via DI

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

            await _storedFileInfoRepo.UploadFileAsync(Upload, user);
            return RedirectToPage();
        }

        // New: soft-delete handler (moves file to wastebasket)
        public async Task<IActionResult> OnPostSoftDeleteAsync([FromForm] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest();

            await _storedFileInfoRepo.SoftDeleteAsync(filePath);
            return RedirectToPage();
        }
       
    }
}
