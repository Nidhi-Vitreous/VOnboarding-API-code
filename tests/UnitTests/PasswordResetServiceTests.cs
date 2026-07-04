using System;
using Microsoft.Extensions.Logging.Abstractions;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Options;
using Vitreous.Onboarding.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Vitreous.Onboarding.UnitTests;

public class PasswordResetServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public async Task RequestPasswordResetAsync_throws_for_invalid_email(string email)
    {
        var emailService = new FakeEmailService();
        var sut = CreateSut(new FakeUserRepository(), new FakePasswordResetTokenRepository(), emailService);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.RequestPasswordResetAsync(email));

        Assert.Equal("Validation failed.", exception.Message);
        Assert.Equal(0, emailService.SendCount);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_returns_generic_success_when_user_not_found()
    {
        var tokenRepository = new FakePasswordResetTokenRepository();
        var emailService = new FakeEmailService();
        var sut = CreateSut(new FakeUserRepository(), tokenRepository, emailService);

        var response = await sut.RequestPasswordResetAsync("missing@example.com");

        Assert.Equal(PasswordResetService.SuccessMessage, response.Message);
        Assert.Equal(0, tokenRepository.AddCount);
        Assert.Equal(0, emailService.SendCount);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_creates_token_and_sends_email_when_user_exists()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            IsActive = true,
        };
        var userRepository = new FakeUserRepository { User = user };
        var tokenRepository = new FakePasswordResetTokenRepository();
        var emailService = new FakeEmailService();
        var sut = CreateSut(userRepository, tokenRepository, emailService);

        var response = await sut.RequestPasswordResetAsync("admin@example.com");

        Assert.Equal(PasswordResetService.SuccessMessage, response.Message);
        Assert.Equal(1, tokenRepository.AddCount);
        Assert.Equal(user.Id, tokenRepository.LastAddedToken?.UserId);
        Assert.True(tokenRepository.LastAddedToken!.ExpiresAt > DateTime.UtcNow.AddMinutes(14));
        Assert.True(tokenRepository.LastAddedToken.ExpiresAt <= DateTime.UtcNow.AddMinutes(21));
        Assert.Equal(1, emailService.SendCount);
        Assert.Equal("admin@example.com", emailService.LastTo);
        Assert.Equal(PasswordResetEmailContent.Subject, emailService.LastSubject);
        Assert.Contains("/reset-password?token=", emailService.LastBody);
        Assert.Contains(tokenRepository.LastAddedToken.Token, emailService.LastBody);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_invalidates_existing_active_tokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            IsActive = true,
        };
        var userRepository = new FakeUserRepository { User = user };
        var tokenRepository = new FakePasswordResetTokenRepository();
        var emailService = new FakeEmailService();
        var sut = CreateSut(userRepository, tokenRepository, emailService);

        await sut.RequestPasswordResetAsync("admin@example.com");
        await sut.RequestPasswordResetAsync("admin@example.com");

        Assert.Equal(2, tokenRepository.AddCount);
        Assert.Equal(2, tokenRepository.InvalidateCount);
        Assert.Equal(2, emailService.SendCount);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_does_not_create_token_or_send_email_for_inactive_user()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            IsActive = false,
        };
        var userRepository = new FakeUserRepository { User = user };
        var tokenRepository = new FakePasswordResetTokenRepository();
        var emailService = new FakeEmailService();
        var sut = CreateSut(userRepository, tokenRepository, emailService);

        var response = await sut.RequestPasswordResetAsync("admin@example.com");

        Assert.Equal(PasswordResetService.SuccessMessage, response.Message);
        Assert.Equal(0, tokenRepository.AddCount);
        Assert.Equal(0, emailService.SendCount);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_returns_success_when_email_send_fails()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            IsActive = true,
        };
        var userRepository = new FakeUserRepository { User = user };
        var tokenRepository = new FakePasswordResetTokenRepository();
        var emailService = new FakeEmailService { ShouldThrow = true };
        var sut = CreateSut(userRepository, tokenRepository, emailService);

        var response = await sut.RequestPasswordResetAsync("admin@example.com");

        Assert.Equal(PasswordResetService.SuccessMessage, response.Message);
        Assert.Equal(1, tokenRepository.AddCount);
        Assert.Equal(1, emailService.SendCount);
    }

    private static PasswordResetService CreateSut(
        FakeUserRepository userRepository,
        FakePasswordResetTokenRepository tokenRepository,
        FakeEmailService emailService) =>
        new(
            userRepository,
            tokenRepository,
            emailService,
            Options.Create(new PasswordResetOptions
            {
                TokenTtlMinutes = 20,
                FrontendBaseUrl = "http://localhost:4200",
            }),
            NullLogger<PasswordResetService>.Instance);

    private sealed class FakeEmailService : IEmailService
    {
        public int SendCount { get; private set; }
        public string? LastTo { get; private set; }
        public string? LastSubject { get; private set; }
        public string? LastBody { get; private set; }
        public bool ShouldThrow { get; init; }

        public Task SendEmailAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            SendCount++;
            LastTo = to;
            LastSubject = subject;
            LastBody = body;

            if (ShouldThrow)
            {
                throw new InvalidOperationException("Simulated email failure.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; init; }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(User);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByIdTrackedWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPageAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default) =>
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
            Task.FromResult(false);

        public Task<bool> RoleNameInUseAsync(string roleName, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IReadOnlyDictionary<string, int>> GetActiveUserCountsByRoleNamesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        public int AddCount { get; private set; }
        public int InvalidateCount { get; private set; }
        public PasswordResetToken? LastAddedToken { get; private set; }

        public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            AddCount++;
            LastAddedToken = token;
            return Task.CompletedTask;
        }

        public Task InvalidateActiveForUserAsync(
            Guid userId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            InvalidateCount++;
            return Task.CompletedTask;
        }

        public Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task MarkAsUsedAsync(
            PasswordResetToken token,
            DateTime utcNow,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
