using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Users;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> RoleNameInUseAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, int>> GetActiveUserCountsByRoleNamesAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    int GetAccessTokenExpiresInSeconds();
}

public interface IUserService
{
    Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserListResponse> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);
    Task<UserCreatedResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
