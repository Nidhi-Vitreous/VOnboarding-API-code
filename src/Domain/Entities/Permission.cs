namespace Vitreous.Onboarding.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
