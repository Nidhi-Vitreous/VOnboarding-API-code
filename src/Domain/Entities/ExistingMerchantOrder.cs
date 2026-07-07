namespace Vitreous.Onboarding.Domain.Entities;

public class ExistingMerchantOrder
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? Product { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
