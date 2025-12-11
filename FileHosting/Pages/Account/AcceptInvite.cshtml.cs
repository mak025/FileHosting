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
using Microsoft.Extensions.Logging;
using FileHostingBackend.Repos;

namespace FileHosting.Pages.Account
{
    [AllowAnonymous]
    public class AcceptInviteModel : PageModel
    {
        private readonly InviteRepo _inviteService;
        private readonly UserService _userService;
        private readonly ILogger<AcceptInviteModel> _logger;

        public AcceptInviteModel(InviteRepo inviteService, UserService userService, ILogger<AcceptInviteModel> logger)
        {
            _inviteService = inviteService;
            _userService = userService;
            _logger = logger;
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
                ErrorMessage = "Invalid or expired invite token.";
                return Page();
            }

            _logger.LogInformation("AcceptInvite: attempting to consume invite for {Email}", Input.Email);

            // Use InviteService to atomically mark the invite used
            var consumeResult = await _inviteService.TryConsumeInviteAsync(Input.Token, Input.Email);
            if (!consumeResult.success)
            {
                ModelState.AddModelError(string.Empty, "Invite either used or expired. " + consumeResult.reason);
                ErrorMessage = "Invite either used or expired. " + consumeResult.reason;
                _logger.LogWarning("AcceptInvite: failed to consume invite for {Email}: {Reason}", Input.Email, consumeResult.reason);
                return Page();
            }

            _logger.LogInformation("AcceptInvite: invite consumed, creating user {Email}", Input.Email);

            // Create user through the service/repo layer.
            try
            {
                _userService.CreateUserAsync(
                    Input.Name,
                    Input.Email,
                    Input.Address ?? string.Empty,
                    Input.PhoneNumber ?? string.Empty,
                    null,
                    0 // Member role
                );
            }
            catch (Exception ex)
            {
                // Log the exception and surface it to the page
                _logger.LogError(ex, "Failed to create user for invite {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Failed to create user: " + ex.Message);
                ErrorMessage = "Failed to create user: " + ex.Message;
                return Page();
            }

            _logger.LogInformation("AcceptInvite: user created successfully for {Email}", Input.Email);

            // Redirect user to login page to sign in
            return LocalRedirect("/Account/Login");
        }
    }
}