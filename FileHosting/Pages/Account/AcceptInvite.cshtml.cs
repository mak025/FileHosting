using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using FileHostingBackend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileHosting.Pages.Account
{
    [AllowAnonymous]
    public class AcceptInviteModel : PageModel
    {
        private readonly InviteService _inviteService;
        private readonly FileHostDBContext _dbContext;

        public AcceptInviteModel(InviteService inviteService, FileHostDBContext dbContext)
        {
            _inviteService = inviteService;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string Token { get; set; }
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }

            public string Address { get; set; }

            public string PhoneNumber { get; set; }

            [Required]
            public string Token { get; set; }
        }

        public IActionResult OnGet(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ErrorMessage = "Invalid invite link.";
                return Page();
            }

            var validation = _inviteService.ValidateToken(token);
            if (!validation.success || !string.Equals(validation.email, email, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = string.IsNullOrEmpty(validation.reason) ? "Invalid or expired invite." : validation.reason;
                return Page();
            }

            Input = new InputModel { Email = email, Token = token };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var validation = _inviteService.ValidateToken(Input.Token);
            if (!validation.success || !string.Equals(validation.email, Input.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired invite token.");
                return Page();
            }

            // Check invite record and mark used
            var invite = _dbContext.Invites.FirstOrDefault(i => i.Token == Input.Token && i.Email == Input.Email && !i.Used);
            if (invite == null || invite.ExpiresAt < DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Invite either used or expired.");
                return Page();
            }

            // Ensure a union exists; use first or create default
            var union = _dbContext.Union.FirstOrDefault();
            if (union == null)
            {
                union = new Union { UnionName = "DefaultUnion" };
                _dbContext.Add(union);
                await _dbContext.SaveChangesAsync();
            }

            // Create member and save
            var member = new Member(Input.Name, Input.Email, Input.Address ?? string.Empty, Input.PhoneNumber ?? string.Empty, union);
            _dbContext.Add(member);

            invite.Used = true;
            await _dbContext.SaveChangesAsync();

            // Optional: sign them in immediately
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, member.Name ?? member.Email),
                new Claim(ClaimTypes.NameIdentifier, member.ID.ToString()),
                new Claim(ClaimTypes.Email, member.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, member.Type.ToString())
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return LocalRedirect("/");
        }
    }
}