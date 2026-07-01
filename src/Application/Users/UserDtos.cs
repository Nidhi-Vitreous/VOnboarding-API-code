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

public sealed class UserDetailDto : UserSummaryDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class UserPaging
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
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
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UserUpdateRequest
{
    public Guid? RoleId { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
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
