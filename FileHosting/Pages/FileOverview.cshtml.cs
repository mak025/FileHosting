using FileHostingBackend.Models;
using FileHostingBackend.Services;
using Microsoft.AspNetCore.Authorization;
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

        // Bind a collection so multiple files can be uploaded
        [BindProperty]
        public List<IFormFile> Upload { get; set; } = new();

        public List<User> AllUsers { get; set; } = new();
        public bool IsAdmin { get; private set; }

        public List<StoredFileInfo> Files { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdSt = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdSt) || !int.TryParse(userIdSt, out var userId))
                return;

            var currentUser = await _dbContext.Users.FirstAsync(u => u.ID == userId);

            IsAdmin = currentUser.Type == FileHostingBackend.Models.User.UserType.Admin
                   || currentUser.Type == FileHostingBackend.Models.User.UserType.SysAdmin;

            Files = await _dbContext.StoredFiles
                .Include(f => f.UsersWithPermission)
                .ToListAsync();

            if (IsAdmin)
            {
                AllUsers = await _dbContext.Users
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }

            // Find the user in the database
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                // Optionally: handle user not found
                Files = new List<StoredFileInfo>();
                return;
            }

            if (User.IsInRole("Admin") || User.IsInRole("SysAdmin"))
            {
                Files = await _storedFileInfoService.GetAllFilesAsync();
            }
            else
            {
                Files = await _storedFileInfoService.GetFilesWithPermissionAsync(userId);
            }

            var searchQuery = Request.Query["search"].ToString();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                Files = Files
                    .Where(f => f.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
      public async Task<IActionResult> OnPostPermissionsAsync([FromForm] int fileId, [FromForm] List<int> userIds)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized();

            var currentUser = await _dbContext.Users.FirstAsync(u => u.ID == currentUserId);

            var isAdmin = currentUser.Type == FileHostingBackend.Models.User.UserType.Admin
                       || currentUser.Type == FileHostingBackend.Models.User.UserType.SysAdmin;

            if (!isAdmin)
                return Forbid();

            var file = await _dbContext.StoredFiles
                .Include(f => f.UsersWithPermission)
                .FirstOrDefaultAsync(f => f.ID == fileId);

            if (file == null)
                return NotFound();

            file.UsersWithPermission.Clear();

            foreach (var userId in userIds)
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                    file.UsersWithPermission.Add(user);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        } 

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Upload == null || !Upload.Any() || Upload.All(f => f == null || f.Length == 0))
            {
                ModelState.AddModelError("Upload", "Venglist vel en fil");
                return Page();
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ModelState.AddModelError("", "Bruger ikke bekræftet.");
                return Page();
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Bruger ikke fundet.");
                return Page();
            }

            try
            {
                // Upload each selected file
                foreach (var file in Upload)
                {
                    if (file == null || file.Length == 0)
                        continue;

                    await _storedFileInfoService.UploadFileAsync(file, user);
                }
            }
            catch (Exception ex)
            {
                // Log or add a model error so the user sees a failure
                ModelState.AddModelError("", "Kunne ikke uploade fil " + ex.Message);
                return Page();
            }

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