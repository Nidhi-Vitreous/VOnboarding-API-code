namespace Vitreous.Onboarding.Application.Users;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public sealed class UserRoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}

public sealed class UserDetailDto : UserSummaryDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<UserRoleDto> Roles { get; set; } = [];
}

public sealed class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public static class UserPaging
{
    public const int DefaultPage = Common.ListPaging.DefaultPage;
    public const int DefaultPageSize = Common.ListPaging.DefaultPageSize;
    public const int MinPage = Common.ListPaging.MinPage;
    public const int MinPageSize = Common.ListPaging.MinPageSize;
    public const int MaxPageSize = Common.ListPaging.MaxPageSize;
}

public sealed class UserListResponse
{
    public IReadOnlyList<UserSummaryDto> Data { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public sealed class UserCreateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? OfficeNumber { get; set; }
    public string? Notes { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
    public bool IsActive { get; set; } = true;
}

public sealed class UserUpdateRequest
{
    public Guid? RoleId { get; set; }
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
}

public sealed class UserStatusUpdateRequest
{
    public bool IsActive { get; set; }
}

public sealed class UserStatusResponse
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
