using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FileHostingBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace FileHosting.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly FileHostDBContext _dbContext;

        public LoginModel(FileHostDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = "/")
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
        {
            ReturnUrl = returnUrl ?? "/";

            if (!ModelState.IsValid)
                return Page();

            // Prototype: authenticate by email only
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name ?? user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                // Role claim uses the enum name
                new Claim(ClaimTypes.Role, user.Type.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect(ReturnUrl);
        }
    }
}