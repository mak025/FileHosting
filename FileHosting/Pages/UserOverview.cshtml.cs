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
    }
}
