using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

internal sealed class FakePermissionAuthorizationService : IPermissionAuthorizationService
{
    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];

    public Task<bool> HasSystemPermissionAsync(
        User user,
        string systemPermission,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(
            GrantedPermissions.Contains(systemPermission, StringComparer.OrdinalIgnoreCase));

    public Task<IReadOnlyList<string>> GetGrantedSystemPermissionNamesAsync(
        User user,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(GrantedPermissions);
}
