using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FileHostingBackend.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileHosting.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class InviteMemberModel : PageModel
    {
        private readonly InviteRepo _inviteService; // Service to handle invites

        public InviteMemberModel(InviteRepo inviteService)
        {
            _inviteService = inviteService;
        }

        [BindProperty]
        public InputModel Input { get; set; } // Bind the input model for form data

        public string StatusMessage { get; set; } // Status message to display

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
            // show empty form
        }

        public async Task<IActionResult> OnPostAsync() // Handle form submission
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Get the current user's ID
            var invitedById = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsed)) // Parse it to int
                invitedById = parsed;

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}"; // Construct base URL

            try
            {
                await _inviteService.CreateAndSendInviteAsync(Input.Email, invitedById, baseUrl, TimeSpan.FromDays(7)); // 7 days expiry
                StatusMessage = $"Invitation sent to {Input.Email}."; // Success message
                ModelState.Clear();
                Input = new InputModel(); // Clear the form
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to send invite: " + ex.Message);
            }

            return Page();
        }
    }
}


