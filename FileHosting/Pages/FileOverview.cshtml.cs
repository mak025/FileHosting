using FileHostingBackend.Models;
using FileHostingBackend.Repos;                       // IStoredFileInfoRepo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;                      // IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


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

        // New: redirect to Minio presigned URL so file is delivered directly from storage
        public async Task<IActionResult> OnGetDownloadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("Missing filePath.");
            }

            // Sanitize object name to prevent path traversal style input
            filePath = Path.GetFileName(filePath);

            // Lookup metadata by FilePath (object name) and ensure not soft-deleted
            var meta = await _dbContext.StoredFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FilePath == filePath);

            if (meta == null || meta.IsSoftDeleted)
            {
                return NotFound();
            }

            // Create a presigned URL that includes Content-Disposition with the original filename.
            var presignedUrl = await _storedFileInfoRepo.GetPresignedDownloadUrlAsync(filePath, meta.Name, TimeSpan.FromMinutes(5));
            if (string.IsNullOrEmpty(presignedUrl))
            {
                return NotFound();
            }

            // Redirect the client directly to Minio. The browser will download directly from Minio
            // (no streaming through the app memory).
            return Redirect(presignedUrl);
        }
    }
}
