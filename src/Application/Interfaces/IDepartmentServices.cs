using Vitreous.Onboarding.Application.Roles;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IDepartmentService
{
    Task<DepartmentListResponse> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentPermissionsResponse?> GetPermissionsAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
