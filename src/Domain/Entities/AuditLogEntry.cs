namespace Vitreous.Onboarding.Domain.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
}
