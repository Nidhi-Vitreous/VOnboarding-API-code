using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Roles;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class RoleServicePagingTests
{
    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, 10, 1, 10)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, -3, 1, 1)]
    [InlineData(1, 150, 1, 100)]
    public async Task GetAllAsync_clamps_page_and_page_size(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize)
    {
        var roleRepository = new FakeRoleRepository();
        var sut = CreateSut(roleRepository);

        var result = await sut.GetAllAsync(requestedPage, requestedPageSize);

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedPage, roleRepository.LastPage);
        Assert.Equal(expectedPageSize, roleRepository.LastPageSize);
    }

    [Theory]
    [InlineData(42, 10, 5)]
    [InlineData(0, 10, 0)]
    public async Task GetAllAsync_computes_total_pages(
        int totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        var roleRepository = new FakeRoleRepository { TotalCount = totalCount };
        var sut = CreateSut(roleRepository);

        var result = await sut.GetAllAsync(1, pageSize);

        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_passes_search_argument_through_to_repository()
    {
        var roleRepository = new FakeRoleRepository();
        var sut = CreateSut(roleRepository);
        const string search = "  manager  ";

        await sut.GetAllAsync(1, 10, search);

        Assert.Equal(search, roleRepository.LastSearch);
    }

    private static RoleService CreateSut(FakeRoleRepository roleRepository) =>
        new(roleRepository, new FakeUserRepository(), new FakeDepartmentRepository());

    private sealed class FakeRoleRepository : IRoleRepository
    {
        public int TotalCount { get; init; }
        public int? LastPage { get; private set; }
        public int? LastPageSize { get; private set; }
        public string? LastSearch { get; private set; }

        public Task<(IReadOnlyList<Role> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default)
        {
            LastPage = page;
            LastPageSize = pageSize;
            LastSearch = search;
            return Task.FromResult<(IReadOnlyList<Role> Items, int TotalCount)>(
                ([], TotalCount));
        }

        public Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<Role>> GetRolesWithPermissionsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role> CreateRoleAsync(
            Role role,
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Role?> UpdateRoleAsync(
            Guid id,
            string name,
            string roleType,
            Guid departmentId,
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> RoleNameExistsAsync(
            string name,
            Guid? excludeRoleId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<Permission>> GetPermissionsByIdsAsync(
            IReadOnlyList<Guid> permissionIds,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<Permission>> GetPermissionsBySystemNamesAsync(
            IReadOnlyList<string> systemNames,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<IReadOnlyDictionary<string, int>> GetActiveUserCountsByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>());

        public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SaveTrackedChangesAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> UsernameExistsAsync(
            string username,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> EmailExistsAsync(
            string email,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<bool> RoleNameInUseAsync(string roleName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeDepartmentRepository : IDepartmentRepository
    {
        public Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
