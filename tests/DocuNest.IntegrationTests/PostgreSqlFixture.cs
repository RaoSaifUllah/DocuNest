using Dapper;
using DocuNest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace DocuNest.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private string baseConnection = "";
    public string Schema { get; } = "dn_test_" + Guid.NewGuid().ToString("N");
    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        baseConnection = Environment.GetEnvironmentVariable("DOCUNEST_TEST_CONNECTION") ?? ReadIgnoredLocalConnection();
        if (string.IsNullOrWhiteSpace(baseConnection))
            throw new InvalidOperationException("Set DOCUNEST_TEST_CONNECTION to a real PostgreSQL test connection. Integration tests cannot run against an in-memory substitute.");
        var builder = new NpgsqlConnectionStringBuilder(baseConnection) { SearchPath = Schema };
        ConnectionString = builder.ConnectionString;
        await using var connection = new NpgsqlConnection(baseConnection);
        await connection.OpenAsync();
        await connection.ExecuteAsync($"CREATE SCHEMA \"{Schema}\"");
        await using var scoped = new NpgsqlConnection(ConnectionString);
        await scoped.OpenAsync();
        await SearchPathGuardInterceptor.SelectAndVerifyAsync(scoped);
        var actualSchema = await scoped.ExecuteScalarAsync<string>("SELECT current_schema()");
        if (!string.Equals(actualSchema, Schema, StringComparison.Ordinal))
            throw new InvalidOperationException($"The database connection did not honor the isolated SearchPath. Expected {Schema}; observed {actualSchema}. No integration migration or data deletion was attempted.");
        await using var db = CreateDb();
        await db.Database.MigrateAsync();
    }

    public DocuNestDbContext CreateDb() => new(new DbContextOptionsBuilder<DocuNestDbContext>()
        .UseNpgsql(ConnectionString).AddInterceptors(new SearchPathGuardInterceptor()).Options);

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await SearchPathGuardInterceptor.SelectAndVerifyAsync(connection);
        await connection.ExecuteAsync("TRUNCATE TABLE users, workspaces CASCADE");
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(baseConnection)) return;
        // The deletion target is generated here; user configuration never becomes an identifier.
        if (Schema.Length != 40 || !Schema.StartsWith("dn_test_", StringComparison.Ordinal))
            throw new InvalidOperationException("Refusing cleanup of an unexpected integration schema.");
        await using var connection = new NpgsqlConnection(baseConnection);
        await connection.OpenAsync();
        await connection.ExecuteAsync($"DROP SCHEMA IF EXISTS \"{Schema}\" CASCADE");
    }

    private static string ReadIgnoredLocalConnection()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Directory.Packages.props")))
            {
                var envPath = Path.Combine(directory.FullName, ".env");
                if (!File.Exists(envPath)) return "";
                foreach (var line in File.ReadLines(envPath))
                {
                    const string prefix = "DOCUNEST_TEST_CONNECTION=";
                    if (line.StartsWith(prefix, StringComparison.Ordinal))
                        return line[prefix.Length..].Trim().Trim('"', '\'');
                }
                return "";
            }
            directory = directory.Parent;
        }
        return "";
    }
}
