using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Merchants;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class MerchantServicePagingTests
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
        var merchantRepository = new FakeMerchantRepository();
        var sut = CreateSut(merchantRepository);

        var result = await sut.GetAllAsync(new MerchantListQuery
        {
            Page = requestedPage,
            PageSize = requestedPageSize,
        });

        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedPage, merchantRepository.LastQuery?.Page);
        Assert.Equal(expectedPageSize, merchantRepository.LastQuery?.PageSize);
    }

    [Theory]
    [InlineData(42, 10, 5)]
    [InlineData(0, 10, 0)]
    public async Task GetAllAsync_computes_total_pages(
        int totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        var merchantRepository = new FakeMerchantRepository { TotalCount = totalCount };
        var sut = CreateSut(merchantRepository);

        var result = await sut.GetAllAsync(new MerchantListQuery { Page = 1, PageSize = pageSize });

        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_trims_search_before_querying_repository()
    {
        var merchantRepository = new FakeMerchantRepository();
        var sut = CreateSut(merchantRepository);

        await sut.GetAllAsync(new MerchantListQuery
        {
            Page = 1,
            PageSize = 10,
            Search = "  acme  ",
        });

        Assert.Equal("acme", merchantRepository.LastQuery?.Search);
    }

    private static MerchantService CreateSut(FakeMerchantRepository merchantRepository) =>
        new(merchantRepository, new FakeUserRepository(), new FakeDepartmentResolver());

    private sealed class FakeMerchantRepository : IMerchantRepository
    {
        public int TotalCount { get; init; }
        public MerchantListQuery? LastQuery { get; private set; }

        public Task<(IReadOnlyList<Merchant> Items, int TotalCount)> GetListAsync(
            MerchantListQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult<(IReadOnlyList<Merchant> Items, int TotalCount)>(
                ([], TotalCount));
        }

        public Task<Merchant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task AddAsync(Merchant merchant, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task UpdateAsync(Merchant merchant, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<MerchantStatusHistory>> GetStatusHistoryAsync(
            Guid merchantId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task AddStatusHistoryAsync(
            MerchantStatusHistory history,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyList<AuditLogEntry>> GetAuditLogAsync(
            Guid merchantId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task AddAuditLogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
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

        public Task<IReadOnlyDictionary<string, int>> GetActiveUserCountsByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task DeleteAsync(User user, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeDepartmentResolver : IDepartmentResolver
    {
        public Task<AuthorizationContext> ResolveAsync(
            User user,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
