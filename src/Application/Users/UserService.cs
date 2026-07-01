using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Users;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
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

    internal static UserSummaryDto MapToSummary(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        IsActive = user.IsActive,
    };

    internal static UserDetailDto MapToDetail(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? user.Username,
        Email = user.Email,
        Role = user.Role,
        Department = user.Department,
        IsActive = user.IsActive,
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
}
