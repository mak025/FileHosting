using System.ComponentModel.DataAnnotations;
using FileHostingBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public InputModel Input { get; set; } // Pre-fill email and token

        public string Token { get; set; } // Invite token
        public string ErrorMessage { get; set; } // Error message to display

        public class InputModel // Form input model
        {
            [Required] // Email is required
            [EmailAddress] // Must be a valid email address
            public string Email { get; set; }

            [Required] // Name is required
            public string Name { get; set; }

            public string Address { get; set; }

            public string PhoneNumber { get; set; }

            [Required] // Token is required
            public string Token { get; set; }
        }

        public IActionResult OnGet(string token, string email) // Accept invite link with token and email
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email)) // Validate parameters
            {
                ErrorMessage = "Invalid invite link.";
                return Page();
            }

            var validation = _inviteService.ValidateToken(token); // Validate the token
            if (!validation.success || !string.Equals(validation.email, email, StringComparison.OrdinalIgnoreCase)) // Check if token is valid and matches email
            {
                ErrorMessage = string.IsNullOrEmpty(validation.reason) ? "Invalid or expired invite." : validation.reason;
                return Page();
            }

            Input = new InputModel { Email = email, Token = token }; // Pre-fill email and token
            return Page();
        }

        public async Task<IActionResult> OnPostAsync() // Handle form submission
        {
            if (!ModelState.IsValid)
                return Page();

            var validation = _inviteService.ValidateToken(Input.Token);
            if (!validation.success || !string.Equals(validation.email, Input.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired invite token."); // Add model error
                ErrorMessage = "Invalid or expired invite token.";
                return Page();
            }

            _logger.LogInformation("AcceptInvite: attempting to consume invite for {Email}", Input.Email);

            // Use InviteService to atomically mark the invite used
            
            _logger.LogInformation("AcceptInvite: invite consumed, creating user {Email}", Input.Email);

            // Create user through the service/repo layer.
            try
            {
                 await _userService.CreateUserAsync( // Create the user
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