using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Users;

public sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : IUserService
{
    private const int MaxUsernameLength = 128;
    private const int MaxEmailLength = 256;
    private const int MaxUsernameDedupAttempts = 10_000;
    private readonly string _emailDomain = configuration["Users:EmailDomain"]
        ?? throw new InvalidOperationException("Users:EmailDomain is not configured.");

    public async Task<UserProfileDto?> GetProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToProfile(user);
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdWithRolesAsync(id, cancellationToken);
        return user is null ? null : MapToDetail(user);
    }

    public async Task<UserDetailDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null && username.Contains('@'))
        {
            user = await userRepository.GetByEmailAsync(username, cancellationToken);
        }

        return user is null ? null : MapToDetail(user);
    }

    public async Task<UserListResponse> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        page = ListPaging.NormalizePage(page);
        pageSize = ListPaging.NormalizePageSize(pageSize);

        var (items, totalCount) = await userRepository.GetPageAsync(page, pageSize, search, cancellationToken);

        return new UserListResponse
        {
            Data = items.Select(MapToSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = ListPaging.ComputeTotalPages(totalCount, pageSize),
        };
    }

    public async Task<UserDetailDto> CreateAsync(
        UserCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        UserValidation.ValidateRequest(request);

        var resolvedRoles = await ResolveRolesAsync(request.RoleIds, cancellationToken);
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var (username, email) = await ResolveUniqueIdentityAsync(firstName, lastName, cancellationToken);
        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = resolvedRoles[0].Name,
            FullName = $"{firstName} {lastName}".Trim(),
            FirstName = firstName,
            LastName = lastName,
            OfficeNumber = request.OfficeNumber?.Trim(),
            Notes = request.Notes?.Trim(),
            TwoFactorEnabled = request.TwoFactorEnabled,
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = null,
            UserRoles = resolvedRoles
                .Select(role => new UserRole { RoleId = role.Id })
                .ToList(),
        };

        await userRepository.AddAsync(user, cancellationToken);

        var detail = MapToDetail(user);
        detail.Roles = MapToRoleDtos(resolvedRoles);
        return detail;
    }

    public async Task<UserDetailDto?> UpdateAsync(
        Guid id,
        UserUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdTrackedWithRolesAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        UserValidation.ValidateUpdateRequest(request);
        var resolvedRoles = await ResolveRolesAsync(request.RoleIds, cancellationToken);

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        user.FirstName = firstName;
        user.LastName = lastName;
        user.FullName = $"{firstName} {lastName}".Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.OfficeNumber = request.OfficeNumber?.Trim();
        user.Notes = request.Notes?.Trim();
        user.TwoFactorEnabled = request.TwoFactorEnabled;
        user.Role = resolvedRoles[0].Name;
        user.UpdatedAt = DateTime.UtcNow;

        SyncUserRoles(user, resolvedRoles);

        await userRepository.SaveTrackedChangesAsync(cancellationToken);

        var detail = MapToDetail(user);
        detail.Roles = MapToRoleDtos(resolvedRoles);
        return detail;
    }

    public async Task<UserStatusResponse?> SetStatusAsync(
        Guid id,
        UserStatusUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);

        return new UserStatusResponse
        {
            Id = user.Id,
            IsActive = user.IsActive,
            UpdatedAt = user.UpdatedAt,
        };
    }

    internal static UserSummaryDto MapToSummary(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        Roles = MapToRoleDtosFromUserRoles(user),
    };

    internal static UserProfileDto MapToProfile(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        UserName = user.Username,
        Email = user.Email,
        Role = user.Role,
        PhoneNumber = user.PhoneNumber,
    };

    internal static UserDetailDto MapToDetail(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        OfficeNumber = user.OfficeNumber,
        Notes = user.Notes,
        TwoFactorEnabled = user.TwoFactorEnabled,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        Roles = MapToRoleDtosFromUserRoles(user),
    };

    internal static AuthUserDto MapToAuthUser(User user) => new()
    {
        Id = user.Id,
        UserName = user.Username,
        Email = user.Email,
        Role = user.Role,
    };

    private static IReadOnlyList<UserRoleDto> MapToRoleDtos(IReadOnlyList<Role> roles) =>
        roles
            .Select(role => new UserRoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DepartmentName = role.Department?.Name ?? string.Empty,
            })
            .ToList();

    private static IReadOnlyList<UserRoleDto> MapToRoleDtosFromUserRoles(User user) =>
        user.UserRoles
            .Where(userRole => userRole.Role is not null)
            .Select(userRole => new UserRoleDto
            {
                Id = userRole.Role!.Id,
                Name = userRole.Role.Name,
                DepartmentName = userRole.Role.Department?.Name ?? string.Empty,
            })
            .ToList();

    private static void SyncUserRoles(User user, IReadOnlyList<Role> resolvedRoles)
    {
        var targetRoleIds = resolvedRoles.Select(role => role.Id).ToHashSet();

        var rolesToRemove = user.UserRoles
            .Where(userRole => !targetRoleIds.Contains(userRole.RoleId))
            .ToList();

        foreach (var userRole in rolesToRemove)
        {
            user.UserRoles.Remove(userRole);
        }

        var existingRoleIds = user.UserRoles.Select(userRole => userRole.RoleId).ToHashSet();

        foreach (var role in resolvedRoles)
        {
            if (!existingRoleIds.Contains(role.Id))
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id });
            }
        }
    }

    private async Task<IReadOnlyList<Role>> ResolveRolesAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var uniqueRoleIds = new List<Guid>();
        var seenRoleIds = new HashSet<Guid>();

        foreach (var roleId in roleIds)
        {
            if (seenRoleIds.Add(roleId))
            {
                uniqueRoleIds.Add(roleId);
            }
        }

        var resolvedRoles = new List<Role>(uniqueRoleIds.Count);

        foreach (var roleId in uniqueRoleIds)
        {
            var role = await roleRepository.GetRoleByIdAsync(roleId, cancellationToken);
            if (role is null)
            {
                throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleNotFound);
            }

            if (!role.IsActive)
            {
                throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleInactive);
            }

            resolvedRoles.Add(role);
        }

        return resolvedRoles;
    }

    private async Task<(string Username, string Email)> ResolveUniqueIdentityAsync(
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var sanitizedFirstName = SanitizeNamePart(firstName);
        var sanitizedLastName = SanitizeNamePart(lastName);

        if (string.IsNullOrEmpty(sanitizedFirstName) || string.IsNullOrEmpty(sanitizedLastName))
        {
            throw new BusinessRuleException("Cannot derive a login from the provided name.");
        }

        var localBase = $"{sanitizedFirstName}.{sanitizedLastName}";

        for (var suffix = 0; suffix <= MaxUsernameDedupAttempts; suffix++)
        {
            var candidateLocalPart = BuildIdentityLocalPart(localBase, suffix);
            var username = candidateLocalPart;
            var email = BuildEmailAddress(candidateLocalPart);

            if (email.Length > MaxEmailLength)
            {
                continue;
            }

            var usernameAvailable = !await userRepository.UsernameExistsAsync(
                username,
                cancellationToken: cancellationToken);
            var emailAvailable = !await userRepository.EmailExistsAsync(
                email,
                cancellationToken: cancellationToken);

            if (usernameAvailable && emailAvailable)
            {
                return (username, email);
            }
        }

        throw new BusinessRuleException(UserMessages.DuplicateUsername);
    }

    private string BuildEmailAddress(string localPart) => $"{localPart}@{_emailDomain}";

    private static string SanitizeNamePart(string value)
    {
        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
            }
            else if (character is >= 'A' and <= 'Z')
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static string BuildIdentityLocalPart(string localBase, int suffix)
    {
        var suffixText = suffix == 0 ? string.Empty : suffix.ToString();
        var maxBaseLength = MaxUsernameLength - suffixText.Length;
        var basePart = localBase.Length > maxBaseLength ? localBase[..maxBaseLength] : localBase;
        return $"{basePart}{suffixText}";
    }
}
