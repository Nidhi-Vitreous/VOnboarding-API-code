namespace Vitreous.Onboarding.Application.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public AuthUserDto User { get; set; } = null!;
}

public sealed class AuthUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
}

public sealed class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class UpdateUsernameRequest
{
    public string NewUserName { get; set; } = string.Empty;
}

public sealed class UserNameResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public sealed class RecoveryResponse
{
    public string Message { get; set; } = string.Empty;
    public string? RecoveryLink { get; set; }
}
