using FileHostingBackend.Models;
using FileHostingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace FileHosting.Pages
{
    [Authorize]
    public class UserOverviewModel : PageModel
    {
        private readonly FileHostDBContext _dbContext;
        private readonly UserService _userService;

        public UserOverviewModel(FileHostDBContext dbContext, UserService userService) // Inject the database context and user service
        {

            _dbContext = dbContext;
            _userService = userService;
        }

        
        public List<User> Users { get; set; } = new();



        public async Task OnGetAsync()
        {
            Users = await _dbContext.Users
                            .Include(u => u.Union)
                            .OrderBy(u => u.Name)
                            .ToListAsync();
        }

        // Deletes a user by id and redirects back to the page
        public async Task<IActionResult> OnPostDeleteAsync([FromForm] int userId) // Handle user deletion
        {
            if (userId <= 0)
                return BadRequest(); // Invalid user ID

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
        
            await _userService.DeleteUserAsync(userId);

            return RedirectToPage();
        }
    }
}
