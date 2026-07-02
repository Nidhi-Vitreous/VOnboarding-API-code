using System.Text.Json;

namespace Vitreous.Onboarding.Application.Merchants;

public static class MerchantStatusNames
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string PendingApproval = "Pending Approval";
    public const string Approved = "Approved";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";

    public static readonly string[] All =
    [
        Draft, Submitted, PendingApproval, Approved, InProgress, Completed, Rejected,
    ];

    private static readonly Dictionary<string, string> LegacyAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Validating"] = PendingApproval,
            ["Waiting Review"] = PendingApproval,
            ["Pending"] = PendingApproval,
        };

    public static bool TryParse(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (All.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            normalized = All.First(name => name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        if (LegacyAliases.TryGetValue(trimmed, out var mapped))
        {
            normalized = mapped;
            return true;
        }

        return false;
    }
}

public class MerchantSummaryDto
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Product { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class MerchantDetailDto : MerchantSummaryDto
{
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class MerchantListResponse
{
    public IReadOnlyList<MerchantSummaryDto> Data { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public sealed class MerchantCreateRequest
{
    public string MerchantName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Guid CreatedBy { get; set; }
    public string? Notes { get; set; }
    public string? Product { get; set; }
}

public sealed class MerchantUpdateRequest
{
    public string? MerchantName { get; set; }
    public string? LegalName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? Product { get; set; }
}

public sealed class MerchantStatusTransitionRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Comment { get; set; }
}

public sealed class MerchantStatusResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public sealed class StatusHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}

public sealed class StatusHistoryListResponse
{
    public IReadOnlyList<StatusHistoryItemDto> Data { get; set; } = [];
}

public sealed class AuditLogItemDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public JsonElement? OldValue { get; set; }
    public JsonElement? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
}

public sealed class AuditLogListResponse
{
    public IReadOnlyList<AuditLogItemDto> Data { get; set; } = [];
}

public sealed class MerchantListQuery
{
    public string? Role { get; init; }
    public string? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
