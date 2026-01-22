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

        public LoginModel(FileHostDBContext dbContext) // Inject the database context
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; } // Bind the input model for form data

        public string ReturnUrl { get; set; } // URL to redirect after login

        public class InputModel // Input model for login form
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = "/") // Handle GET requests
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = "/") // Handle POST requests for login
        {
            ReturnUrl = returnUrl ?? "/"; // Default return URL

            if (!ModelState.IsValid) // Validate the model
                return Page();

            // Prototype: authenticate by email only
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found"); 
                return Page();
            }

            var claims = new List<Claim> // Create claims for the user
            {
                new Claim(ClaimTypes.Name, user.Name ?? user.Email), // Use email as fallback for name
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()), // User ID claim
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty), // Email claim
                // Role claim uses the enum name as string
                new Claim(ClaimTypes.Role, user.Type.ToString())
            };
             
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // Create identity with claims
            var principal = new ClaimsPrincipal(identity); // Create principal

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); // Sign in the user

            return LocalRedirect(ReturnUrl);
        }
    }
}