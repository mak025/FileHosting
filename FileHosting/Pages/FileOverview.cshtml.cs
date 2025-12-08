using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using FileHostingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FileHosting.Pages
{
    [Authorize]
    public class FileOverviewModel : PageModel
    {
        private readonly StoredFileInfoService _storedFileInfoService;
        private readonly FileHostDBContext _dbContext;

        public FileOverviewModel(StoredFileInfoService storedFileInfoService, FileHostDBContext dbContext)
        {
            _storedFileInfoService = storedFileInfoService ?? throw new ArgumentNullException(nameof(storedFileInfoService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public List<StoredFileInfo> Files { get; set; } = new();

        public async Task OnGetAsync()
        {
            Files = await _storedFileInfoService.GetAllFilesAsync();

            var searchQuery = Request.Query["search"].ToString();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                Files = Files
                    .Where(f => f.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Upload == null || Upload.Length == 0)
            {
                ModelState.AddModelError("Upload", "Please select a file");
                return Page();
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ModelState.AddModelError("", "User not authenticated.");
                return Page();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

            await _storedFileInfoService.UploadFileAsync(Upload, user);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSoftDeleteAsync([FromForm] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest();

            await _storedFileInfoService.SoftDeleteAsync(filePath);
            return RedirectToPage();
        }

        // Server-streaming download handler: returns FileStreamResult and prompts Save dialog
        public async Task<IActionResult> OnGetDownloadAsync([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest();

            // find stored metadata (original filename)
            var meta = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == filePath);
            if (meta == null)
                return NotFound();

            var (stream, contentType) = await _storedFileInfoService.GetObjectWithContentTypeAsync(filePath);
            if (stream == null)
                return NotFound();

            // Return FileStreamResult; browser will prompt a download dialog.
            return File(stream, contentType ?? "application/octet-stream", meta.Name);
        }
    }
}