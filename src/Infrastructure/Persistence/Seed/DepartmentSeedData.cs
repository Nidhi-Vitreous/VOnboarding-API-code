namespace Vitreous.Onboarding.Infrastructure.Persistence.Seed;

internal static class DepartmentSeedData
{
    internal sealed record DepartmentSeedEntry(Guid Id, string Name);

    internal static readonly DepartmentSeedEntry[] DefaultDepartments =
    [
        new(Guid.Parse("b1000001-0000-4000-8000-000000000001"), "Sales"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000002"), "Filing"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000003"), "Terminal"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000004"), "Billing"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000005"), "Shipping"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000006"), "Support"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000007"), "Installation"),
        new(Guid.Parse("b1000001-0000-4000-8000-000000000008"), "Admin"),
    ];
}
