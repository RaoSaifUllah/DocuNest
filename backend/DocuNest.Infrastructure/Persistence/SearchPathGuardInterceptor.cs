using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace DocuNest.Infrastructure.Persistence;

/// <summary>Fail closed if a proxy ignores the configured schema. Never silently operate in public.</summary>
public sealed partial class SearchPathGuardInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        var path = ConfiguredPath(connection.ConnectionString);
        if (path is null) return;
        RejectSessionPooler(connection.ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT set_config('search_path', @path, false)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "path";
        parameter.Value = path.Value.QuotedPath;
        command.Parameters.Add(parameter);
        command.ExecuteScalar();
        command.CommandText = "SELECT current_schema()";
        command.Parameters.Clear();
        Verify(path.Value.Expected, command.ExecuteScalar() as string);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RejectSessionPooler(connection.ConnectionString);
        await SelectAndVerifyAsync(connection, cancellationToken: cancellationToken);
    }

    public static async Task SelectAndVerifyAsync(DbConnection connection, DbTransaction? transaction = null,
        bool local = false, CancellationToken cancellationToken = default)
    {
        var path = ConfiguredPath(connection.ConnectionString);
        if (path is null) return;
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = local
            ? "SELECT set_config('search_path', @path, true)"
            : "SELECT set_config('search_path', @path, false)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "path";
        parameter.Value = path.Value.QuotedPath;
        command.Parameters.Add(parameter);
        await command.ExecuteScalarAsync(cancellationToken);
        command.CommandText = "SELECT current_schema()";
        command.Parameters.Clear();
        Verify(path.Value.Expected, await command.ExecuteScalarAsync(cancellationToken) as string);
    }

    private static (string Expected, string QuotedPath)? ConfiguredPath(string connectionString)
    {
        var path = new NpgsqlConnectionStringBuilder(connectionString).SearchPath;
        if (string.IsNullOrWhiteSpace(path)) return null;
        var schemas = path.Split(',').Select(value => value.Trim().Trim('"')).ToArray();
        if (schemas.Any(value => !IdentifierRegex().IsMatch(value)))
            throw new InvalidOperationException("DocuNest schema names must use letters, digits and underscores, starting with a letter or underscore.");
        return (schemas[0], string.Join(',', schemas.Select(schema => '"' + schema + '"')));
    }

    private static void RejectSessionPooler(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(builder.SearchPath) && builder.Host?.Contains("-pooler.", StringComparison.OrdinalIgnoreCase) == true)
            throw new InvalidOperationException("EF writes and migrations with a configured schema require the direct PostgreSQL endpoint. Transaction-pooler session state is unsupported.");
    }

    private static void Verify(string expected, string? actual)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidOperationException("The PostgreSQL connection did not honor its configured schema. Use a connection supporting schema session state, and create the configured schema before migration.");
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();
}
