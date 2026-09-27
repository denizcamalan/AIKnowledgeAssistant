using AIKnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _uploadRoot;

    public CustomWebApplicationFactory()
    {
        _uploadRoot = Path.Combine(Path.GetTempPath(), "aka-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_uploadRoot);
    }

    public static bool PostgresTestsEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("AKA_RUN_POSTGRES_TESTS"), "1", StringComparison.Ordinal);

    public void EnsureDatabaseMigrated()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = ResolveConnectionString();
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DocumentStorage:RootPath"] = _uploadRoot,
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Jwt:SigningKey"] = "integration-test-signing-key-min-32-chars",
            });
        });
    }

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        if (PostgresTestsEnabled)
        {
            return "Host=localhost;Port=5432;Database=aiknowledgeassistant;Username=aka;Password=aka_dev_password";
        }

        return "Host=localhost;Port=5432;Database=aka_integration_placeholder;Username=aka;Password=aka_dev_password";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_uploadRoot))
        {
            try
            {
                Directory.Delete(_uploadRoot, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup for temp upload folder.
            }
        }

        base.Dispose(disposing);
    }
}
