using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


namespace FileHosting.Pages
{
    [Authorize]
    public class UserOverviewModel : PageModel
    {
        private readonly FileHostDBContext _dbContext;

        public UserOverviewModel(FileHostDBContext dbContext)
        {

            _dbContext = dbContext;
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
        public async Task<IActionResult> OnPostDeleteAsync([FromForm] int userId)
        {
            if (userId <= 0)
                return BadRequest();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
