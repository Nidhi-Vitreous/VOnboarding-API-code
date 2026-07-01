using System.Security.Cryptography;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Users;

public sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher) : IUserService
{
    private const int MaxUsernameLength = 128;
    private const int MaxUsernameDedupAttempts = 10_000;
    private const int TemporaryPasswordByteLength = 24;

    public async Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
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
        page = page < UserPaging.MinPage ? UserPaging.DefaultPage : page;
        pageSize = Math.Clamp(pageSize, UserPaging.MinPageSize, UserPaging.MaxPageSize);

        var (items, totalCount) = await userRepository.GetPageAsync(page, pageSize, search, cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new UserListResponse
        {
            Data = items.Select(MapToSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }

    public async Task<UserCreatedResponse> CreateAsync(
        UserCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        UserValidation.ValidateRequest(request);

        var role = await roleRepository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleNotFound);
        }

        if (!role.IsActive)
        {
            throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleInactive);
        }

        var email = request.Email.Trim();
        var username = await ResolveUniqueUsernameAsync(email, cancellationToken);
        var temporaryPassword = GenerateTemporaryPassword();
        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(temporaryPassword),
            Role = role.Name,
            FullName = request.FullName.Trim(),
            Department = request.Department?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = null,
        };

        await userRepository.AddAsync(user, cancellationToken);

        return new UserCreatedResponse
        {
            User = MapToDetail(user),
            TemporaryPassword = temporaryPassword,
        };
    }

    public async Task<UserDetailDto?> UpdateAsync(
        Guid id,
        UserUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (request.RoleId.HasValue)
        {
            var role = await roleRepository.GetRoleByIdAsync(request.RoleId.Value, cancellationToken);
            if (role is null)
            {
                throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleNotFound);
            }

            if (!role.IsActive)
            {
                throw new BusinessRuleException(UserMessages.InvalidRole, UserMessages.RoleInactive);
            }

            user.Role = role.Name;
        }

        if (request.Department is not null)
        {
            user.Department = request.Department.Trim();
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber.Trim();
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);

        return MapToDetail(user);
    }

    internal static UserSummaryDto MapToSummary(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
    };

    internal static UserDetailDto MapToDetail(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };

    internal static AuthUserDto MapToAuthUser(User user) => new()
    {
        Id = user.Id,
        UserName = user.Username,
        Email = user.Email,
        Role = user.Role,
    };

    private async Task<string> ResolveUniqueUsernameAsync(string email, CancellationToken cancellationToken)
    {
        var localPart = email.Split('@')[0].Trim().ToLowerInvariant();

        for (var suffix = 0; suffix <= MaxUsernameDedupAttempts; suffix++)
        {
            var candidate = BuildUsernameCandidate(localPart, suffix);
            if (!await userRepository.UsernameExistsAsync(candidate, cancellationToken: cancellationToken))
            {
                return candidate;
            }
        }

        throw new BusinessRuleException(UserMessages.DuplicateUsername);
    }

    private static string BuildUsernameCandidate(string localPart, int suffix)
    {
        var suffixText = suffix == 0 ? string.Empty : suffix.ToString();
        var maxBaseLength = MaxUsernameLength - suffixText.Length;
        var basePart = localPart.Length > maxBaseLength ? localPart[..maxBaseLength] : localPart;
        return $"{basePart}{suffixText}";
    }

    private static string GenerateTemporaryPassword() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(TemporaryPasswordByteLength))
            .TrimEnd('=')
            .Replace('+', 'x')
            .Replace('/', 'y');
}
