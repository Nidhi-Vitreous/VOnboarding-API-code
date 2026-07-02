namespace Vitreous.Onboarding.Domain.Entities;

public class Merchant
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? Product { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<MerchantStatusHistory> StatusHistory { get; set; } = [];
}
