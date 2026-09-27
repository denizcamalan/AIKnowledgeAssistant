using AIKnowledgeAssistant.Api.LifetimeLab;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.UnitTests.LifetimeLab;

public sealed class CaptiveDependencyTests
{
    [Fact]
    public void SingletonConsumingScoped_ThrowsWhenValidatingScopes()
    {
        var services = new ServiceCollection();
        services.AddCaptiveDependencyAntiPattern();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<CaptiveDependencyConsumer>());

        Assert.Contains("Cannot consume scoped service", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(RequestScopedClock), exception.Message, StringComparison.Ordinal);
    }
}
