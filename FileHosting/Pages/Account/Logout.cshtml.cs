// File: FileHosting/Pages/Account/Logout.cshtml.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FileHosting.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Keep GET as a simple redirect (avoid performing sign-out on GET if you prefer POST-only)
            return RedirectToPage("/Index");
        }

        [ValidateAntiForgeryToken] // Ensure CSRF protection for POST requests
        public async Task<IActionResult> OnPostAsync() // Handle POST requests for logout
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Index");
        }
    }
}