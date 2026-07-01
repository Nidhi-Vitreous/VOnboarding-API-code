namespace Vitreous.Onboarding.Application.Roles;

public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class DepartmentListResponse
{
    public IReadOnlyList<DepartmentDto> Data { get; set; } = [];
}

public sealed class DepartmentPermissionItemDto
{
    public Guid Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class DepartmentPermissionGroupDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<DepartmentPermissionItemDto> Permissions { get; set; } = [];
}

public sealed class DepartmentPermissionsResponse
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public IReadOnlyList<DepartmentPermissionGroupDto> Groups { get; set; } = [];
}
