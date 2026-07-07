using System.Text.Json;
using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Merchants;

public sealed class MerchantService(
    IMerchantRepository merchantRepository,
    IUserRepository userRepository,
    IDepartmentResolver departmentResolver) : IMerchantService
{
    public async Task<MerchantListResponse> GetAllAsync(
        MerchantListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = ListPaging.NormalizePage(query.Page);
        var pageSize = ListPaging.NormalizePageSize(query.PageSize);
        var normalizedQuery = NormalizeListQuery(query, page, pageSize);
        var (items, totalCount) = await merchantRepository.GetListAsync(normalizedQuery, cancellationToken);

        return new MerchantListResponse
        {
            Data = items.Select(MerchantMapper.ToSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = ListPaging.ComputeTotalPages(totalCount, pageSize),
        };
    }

    public async Task<MerchantDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var merchant = await merchantRepository.GetByIdAsync(id, cancellationToken);
        return merchant is null ? null : MerchantMapper.ToDetail(merchant);
    }

    public async Task<MerchantDetailDto> CreateAsync(
        MerchantCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        MerchantValidation.ValidateCreateRequest(request);

        var creator = await userRepository.GetByIdAsync(request.CreatedBy, cancellationToken);
        if (creator is null)
        {
            throw new BusinessRuleException(MerchantMessages.CreatorNotFound);
        }

        var now = DateTime.UtcNow;
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            MerchantName = request.MerchantName.Trim(),
            LegalName = request.LegalName?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            Notes = request.Notes?.Trim(),
            Product = request.Product?.Trim(),
            Status = MerchantStatusNames.Draft,
            Role = creator.Role,
            CreatedBy = creator.Id,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await merchantRepository.AddAsync(merchant, cancellationToken);

        await merchantRepository.AddStatusHistoryAsync(new MerchantStatusHistory
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            OldStatus = null,
            NewStatus = merchant.Status,
            ChangedBy = creator.Id,
            ChangedAt = now,
            Comment = "Merchant created.",
        }, cancellationToken);

        await merchantRepository.AddAuditLogAsync(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Merchant",
            EntityId = merchant.Id,
            Action = "CREATE",
            OldValueJson = null,
            NewValueJson = JsonSerializer.Serialize(MerchantMapper.ToDetail(merchant)),
            CreatedAt = now,
            UserId = creator.Id,
        }, cancellationToken);

        return MerchantMapper.ToDetail(merchant);
    }

    public async Task<MerchantDetailDto?> UpdateAsync(
        Guid id,
        MerchantUpdateRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default)
    {
        var merchant = await merchantRepository.GetByIdAsync(id, cancellationToken);
        if (merchant is null)
        {
            return null;
        }

        var actingUser = await userRepository.GetByIdAsync(actingUserId, cancellationToken);
        if (actingUser is null)
        {
            throw new BusinessRuleException(MerchantMessages.ActingUserNotFound);
        }

        var isAdmin = await IsAdminAsync(actingUser, cancellationToken);
        MerchantValidation.ValidateUpdateAllowed(merchant, isAdmin);

        var before = MerchantMapper.ToDetail(merchant);

        if (!string.IsNullOrWhiteSpace(request.MerchantName))
        {
            merchant.MerchantName = request.MerchantName.Trim();
        }

        if (request.LegalName is not null)
        {
            merchant.LegalName = request.LegalName.Trim();
        }

        if (request.Email is not null)
        {
            merchant.Email = request.Email.Trim();
        }

        if (request.Phone is not null)
        {
            merchant.Phone = request.Phone.Trim();
        }

        if (request.Address is not null)
        {
            merchant.Address = request.Address.Trim();
        }

        if (request.Notes is not null)
        {
            merchant.Notes = request.Notes.Trim();
        }

        if (request.Product is not null)
        {
            merchant.Product = string.IsNullOrWhiteSpace(request.Product)
                ? null
                : request.Product.Trim();
        }

        merchant.UpdatedAt = DateTime.UtcNow;
        await merchantRepository.UpdateAsync(merchant, cancellationToken);

        var after = MerchantMapper.ToDetail(merchant);
        await merchantRepository.AddAuditLogAsync(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Merchant",
            EntityId = merchant.Id,
            Action = "UPDATE",
            OldValueJson = JsonSerializer.Serialize(before),
            NewValueJson = JsonSerializer.Serialize(after),
            CreatedAt = merchant.UpdatedAt,
            UserId = actingUserId,
        }, cancellationToken);

        return after;
    }

    public async Task<MerchantStatusResponse?> TransitionStatusAsync(
        Guid id,
        MerchantStatusTransitionRequest request,
        Guid actingUserId,
        CancellationToken cancellationToken = default)
    {
        var merchant = await merchantRepository.GetByIdAsync(id, cancellationToken);
        if (merchant is null)
        {
            return null;
        }

        var newStatus = MerchantValidation.ValidateStatusTransition(request.NewStatus);
        var now = DateTime.UtcNow;
        var oldStatus = merchant.Status;
        merchant.Status = newStatus;
        merchant.UpdatedAt = now;

        await merchantRepository.UpdateAsync(merchant, cancellationToken);

        var comment = BuildTransitionComment(request);
        await merchantRepository.AddStatusHistoryAsync(new MerchantStatusHistory
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = actingUserId,
            ChangedAt = now,
            Comment = comment,
        }, cancellationToken);

        await merchantRepository.AddAuditLogAsync(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Merchant",
            EntityId = merchant.Id,
            Action = "STATUS_CHANGE",
            OldValueJson = JsonSerializer.Serialize(new { status = oldStatus }),
            NewValueJson = JsonSerializer.Serialize(new { status = newStatus, comment }),
            CreatedAt = now,
            UserId = actingUserId,
        }, cancellationToken);

        return new MerchantStatusResponse
        {
            Id = merchant.Id,
            Status = merchant.Status,
            UpdatedAt = merchant.UpdatedAt,
        };
    }

    public async Task<StatusHistoryListResponse?> GetStatusHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (await merchantRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return null;
        }

        var history = await merchantRepository.GetStatusHistoryAsync(id, cancellationToken);
        return new StatusHistoryListResponse
        {
            Data = history.Select(MerchantMapper.ToHistoryItem).ToList(),
        };
    }

    public async Task<AuditLogListResponse?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await merchantRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return null;
        }

        var entries = await merchantRepository.GetAuditLogAsync(id, cancellationToken);
        return new AuditLogListResponse
        {
            Data = entries.Select(MapAuditLogItem).ToList(),
        };
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        merchantRepository.DeleteAsync(id, cancellationToken);

    private static MerchantListQuery NormalizeListQuery(MerchantListQuery query, int page, int pageSize) =>
        new()
        {
            Role = string.IsNullOrWhiteSpace(query.Role) ? null : query.Role.Trim(),
            Status = string.IsNullOrWhiteSpace(query.Status) ? null : query.Status.Trim(),
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Page = page,
            PageSize = pageSize,
        };

    private async Task<bool> IsAdminAsync(User user, CancellationToken cancellationToken)
    {
        var context = await departmentResolver.ResolveAsync(user, cancellationToken);
        return context.IsAdmin;
    }

    private static string? BuildTransitionComment(MerchantStatusTransitionRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            parts.Add(request.Reason.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            parts.Add(request.Comment.Trim());
        }

        return parts.Count == 0 ? null : string.Join(" — ", parts);
    }

    private static AuditLogItemDto MapAuditLogItem(AuditLogEntry entry) => new()
    {
        Id = entry.Id,
        EntityType = entry.EntityType,
        EntityId = entry.EntityId,
        Action = entry.Action,
        OldValue = ParseJson(entry.OldValueJson),
        NewValue = ParseJson(entry.NewValueJson),
        CreatedAt = entry.CreatedAt,
        UserId = entry.UserId,
    };

    private static JsonElement? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
