namespace Vitreous.Onboarding.Application.Users;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UserDetailDto : UserSummaryDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class UserListResponse
{
    public IReadOnlyList<UserSummaryDto> Data { get; set; } = [];
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
