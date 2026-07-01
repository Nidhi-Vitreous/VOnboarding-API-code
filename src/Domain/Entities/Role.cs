namespace Vitreous.Onboarding.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
