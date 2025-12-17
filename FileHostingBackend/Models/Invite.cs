namespace FileHostingBackend.Models
{
    public class Invite
    {
        public int Id { get; private set; }
        public string InviteeEmail { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        // FK column (int) matching DB
        public int InvitedById { get; set; }
        // navigation property
        public User InvitedBy { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}