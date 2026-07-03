namespace Vitreous.Onboarding.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsUsed => UsedAt.HasValue;

    public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;

    public bool IsValid(DateTime utcNow) => !IsUsed && !IsExpired(utcNow);
}
