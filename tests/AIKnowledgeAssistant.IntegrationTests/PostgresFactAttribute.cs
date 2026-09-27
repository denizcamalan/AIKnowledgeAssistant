namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("AKA_RUN_POSTGRES_TESTS"),
                "1",
                StringComparison.Ordinal))
        {
            Skip = "Set AKA_RUN_POSTGRES_TESTS=1 and run Docker Postgres to execute database integration tests.";
        }
    }
}
