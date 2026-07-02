namespace Vitreous.Onboarding.Domain.Entities;

public class MerchantStatusHistory
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }

    public Merchant Merchant { get; set; } = null!;
}
