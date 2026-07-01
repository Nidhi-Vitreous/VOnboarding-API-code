namespace Vitreous.Onboarding.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Role> Roles { get; set; } = [];
}
