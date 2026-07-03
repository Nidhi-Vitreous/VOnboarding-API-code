using System.Net;
using System.Net.Http.Json;
using Vitreous.Onboarding.Application.Auth;

namespace Vitreous.Onboarding.IntegrationTests;

public class ForgotPasswordApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ForgotPasswordApiTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task ForgotPassword_returns_success_for_unknown_email()
    {
        using var response = await PostForgotPasswordAsync("unknown@example.com");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(body);
        Assert.Equal(PasswordResetService.SuccessMessage, body.Message);
    }

    [Fact]
    public async Task ForgotPassword_returns_validation_error_for_invalid_email()
    {
        using var response = await PostForgotPasswordAsync("not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostForgotPasswordAsync(string email) =>
        _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest { Email = email });
}
