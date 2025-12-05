using System;
using System.Linq;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FileHostingBackend.Services
{
    public class InviteService
    {
        private readonly FileHostDBContext _db;
        private readonly IDataProtector _protector;
        private readonly EmailSettings _emailSettings;

        public InviteService(FileHostDBContext db, IDataProtectionProvider dpProvider, IOptions<EmailSettings> emailOptions)
        {
            _db = db;
            _protector = dpProvider.CreateProtector("InviteTokens.v1");
            _emailSettings = emailOptions.Value;
        }

        public async Task<string> CreateAndSendInviteAsync(string email, int invitedByUserId, string baseUrl, TimeSpan validFor)
        {
            var expires = DateTimeOffset.UtcNow.Add(validFor);
            // Create a token payload and protect it
            var payload = $"{email}|{Guid.NewGuid():N}|{expires.ToUnixTimeSeconds()}";
            var protectedToken = _protector.Protect(payload);
            // Make token URL safe
            var urlToken = System.Web.HttpUtility.UrlEncode(protectedToken);

            var invite = new Invite
            {
                Email = email,
                Token = protectedToken,
                ExpiresAt = expires,
                InvitedById = invitedByUserId,
            };

            _db.Add(invite);
            await _db.SaveChangesAsync();

            var acceptUrl = $"{baseUrl.TrimEnd('/')}/Account/AcceptInvite?token={urlToken}&email={System.Web.HttpUtility.UrlEncode(email)}";

            await SendInviteEmailAsync(email, acceptUrl);

            return invite.Token;
        }

        private async Task SendInviteEmailAsync(string toEmail, string acceptUrl)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_emailSettings.Sender));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = "You're invited � create your account";

            var body = $@"
                <p>Hello,</p>
                <p>You have been invited to sign up. Click the link below to accept the invitation:</p>
                <p><a href=""{acceptUrl}"">Accept invitation</a></p>
                <p>If you did not expect this, ignore this email.</p>";

            msg.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SMTPServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }

        /// <summary>
        /// Validate the token payload (unprotect + check expiry). Returns (success, email, reason).
        /// </summary>
        public (bool success, string email, string reason) ValidateToken(string urlEncodedToken)
        {
            try
            {
                var protectedToken = System.Web.HttpUtility.UrlDecode(urlEncodedToken);
                var payload = _protector.Unprotect(protectedToken);
                // payload format: email|guid|expiresUnix
                var parts = payload.Split('|');
                if (parts.Length != 3) return (false, string.Empty, "Invalid token payload");

                var email = parts[0];
                if (!long.TryParse(parts[2], out var unix)) return (false, string.Empty, "Invalid expiry");

                var expires = DateTimeOffset.FromUnixTimeSeconds(unix);
                if (DateTimeOffset.UtcNow > expires) return (false, string.Empty, "Token expired");

                // Optionally check DB that token exists and not used
                // var invite = _db.Set<Invite>().FirstOrDefault(i => i.Token == protectedToken && i.Email == email);
                // validate invite != null && !invite.Used

                return (true, email, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Atomically mark an invite as used if it exists, matches the email and is not expired/used.
        /// Accepts the URL-encoded token (same value sent in the invite link).
        /// Returns (success, reason).
        /// </summary>
        public async Task<(bool success, string reason)> TryConsumeInviteAsync(string urlEncodedToken, string email)
        {
            try
            {
                var protectedToken = System.Web.HttpUtility.UrlDecode(urlEncodedToken);
                var invite = _db.Invites.FirstOrDefault(i => i.Token == protectedToken && i.Email == email && !i.Used);
                if (invite == null)
                    return (false, "Invite not found or already used.");

                if (invite.ExpiresAt < DateTimeOffset.UtcNow)
                    return (false, "Invite expired.");

                invite.Used = true;
                await _db.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}