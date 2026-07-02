using Vitreous.Onboarding.Application.Merchants;

namespace Vitreous.Onboarding.UnitTests;

public class MerchantStatusNamesTests
{
    [Theory]
    [InlineData("Pending Approval", "Pending Approval")]
    [InlineData("pending approval", "Pending Approval")]
    [InlineData("Validating", "Pending Approval")]
    [InlineData("Waiting Review", "Pending Approval")]
    [InlineData("Pending", "Pending Approval")]
    public void TryParse_accepts_canonical_and_legacy_status_values(string input, string expected)
    {
        var parsed = MerchantStatusNames.TryParse(input, out var normalized);

        Assert.True(parsed);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    public void TryParse_rejects_unknown_status_values(string input)
    {
        var parsed = MerchantStatusNames.TryParse(input, out _);

        Assert.False(parsed);
    }
}
