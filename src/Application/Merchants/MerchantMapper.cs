using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Merchants;

internal static class MerchantMapper
{
    internal static MerchantSummaryDto ToSummary(Merchant merchant) => new()
    {
        Id = merchant.Id,
        MerchantName = merchant.MerchantName,
        Status = merchant.Status,
        Role = merchant.Role,
        Product = merchant.Product,
        CreatedAt = merchant.CreatedAt,
    };

    internal static MerchantDetailDto ToDetail(Merchant merchant) => new()
    {
        Id = merchant.Id,
        MerchantName = merchant.MerchantName,
        Status = merchant.Status,
        Role = merchant.Role,
        Product = merchant.Product,
        CreatedAt = merchant.CreatedAt,
        LegalName = merchant.LegalName,
        Email = merchant.Email,
        Phone = merchant.Phone,
        Address = merchant.Address,
        Notes = merchant.Notes,
        UpdatedAt = merchant.UpdatedAt,
    };

    internal static StatusHistoryItemDto ToHistoryItem(MerchantStatusHistory history) => new()
    {
        Id = history.Id,
        MerchantId = history.MerchantId,
        OldStatus = history.OldStatus,
        NewStatus = history.NewStatus,
        ChangedBy = history.ChangedBy,
        ChangedAt = history.ChangedAt,
        Comment = history.Comment,
    };
}

internal static class MerchantValidation
{
    internal static void ValidateCreateRequest(MerchantCreateRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.MerchantName))
        {
            errors.Add("Merchant name is required.");
        }

        if (request.CreatedBy == Guid.Empty)
        {
            errors.Add("Created by user is required.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessRuleException("Validation failed.", errors.ToArray());
        }
    }

    internal static string ValidateStatusTransition(string? newStatus)
    {
        if (!MerchantStatusNames.TryParse(newStatus, out var normalized))
        {
            throw new BusinessRuleException(MerchantMessages.InvalidStatus);
        }

        return normalized;
    }

    internal static void ValidateUpdateAllowed(Merchant merchant, bool isAdmin)
    {
        if (isAdmin)
        {
            return;
        }

        if (!string.Equals(merchant.Status, MerchantStatusNames.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(MerchantMessages.CannotUpdate, MerchantMessages.DraftOnlyUpdate);
        }
    }
}
