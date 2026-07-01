using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.UnitTests;

public class AssemblyReferenceTests
{
    [Fact]
    public void Domain_does_not_reference_Application_or_Infrastructure()
    {
        var references = typeof(User).Assembly.GetReferencedAssemblies();

        Assert.All(references, reference =>
        {
            Assert.DoesNotContain("Application", reference.Name ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain("Infrastructure", reference.Name ?? string.Empty, StringComparison.Ordinal);
        });
    }
}
