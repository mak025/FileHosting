using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using FileHostingBackend.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileHosting.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class InviteMemberModel : PageModel
    {
        private readonly InviteRepo _inviteService;

        public InviteMemberModel(InviteRepo inviteService)
        {
            _inviteService = inviteService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string StatusMessage { get; set; }

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var invitedById = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsed))
                invitedById = parsed;

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            try
            {
                await _inviteService.CreateAndSendInviteAsync(Input.Email, invitedById, baseUrl, TimeSpan.FromDays(7));
                StatusMessage = $"Invitation sent to {Input.Email}.";
                ModelState.Clear();
                Input = new InputModel();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to send invite: " + ex.Message);
            }

            return Page();
        }
    }
}