namespace Vitreous.Onboarding.Application.Roles;

public sealed class RoleCreateDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? RoleType { get; set; }
    public IReadOnlyList<Guid> PermissionIds { get; set; } = [];
}

public sealed class RoleUpdateDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? RoleType { get; set; }
    public IReadOnlyList<Guid> PermissionIds { get; set; } = [];
}

public sealed class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int ActiveUserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<PermissionDto> Permissions { get; set; } = [];
}

public sealed class RoleListResponse
{
    public IReadOnlyList<RoleDto> Data { get; set; } = [];
}
