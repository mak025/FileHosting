using System;

namespace FileHostingBackend.Models
{
    public class Invite
    {
        public int Id { get; private set; }
        public User Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public User InvitedById { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}