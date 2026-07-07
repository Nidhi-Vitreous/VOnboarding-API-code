namespace Vitreous.Onboarding.Application.ExistingMerchantOrders;

public static class ExistingMerchantOrderStatusNames
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Completed = "Completed";
}

public class ExistingMerchantOrderSummaryDto
{
    public Guid Id { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Product { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ExistingMerchantOrderDetailDto : ExistingMerchantOrderSummaryDto
{
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class ExistingMerchantOrderListResponse
{
    public IReadOnlyList<ExistingMerchantOrderSummaryDto> Data { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public sealed class ExistingMerchantOrderCreateRequest
{
    public string MerchantName { get; set; } = string.Empty;
    public string? Product { get; set; }
    public string? Notes { get; set; }
}

public sealed class ExistingMerchantOrderUpdateRequest
{
    public string? MerchantName { get; set; }
    public string? Product { get; set; }
    public string? Notes { get; set; }
}

public sealed class ExistingMerchantOrderListQuery
{
    public string? Search { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
