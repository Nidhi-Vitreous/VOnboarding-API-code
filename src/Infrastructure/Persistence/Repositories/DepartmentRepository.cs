using Microsoft.EntityFrameworkCore;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Infrastructure.Persistence.Repositories;

public sealed class DepartmentRepository(ApplicationDbContext dbContext) : IDepartmentRepository
{
    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

    public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
}
