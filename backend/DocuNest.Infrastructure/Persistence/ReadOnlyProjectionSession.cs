using System.Data;
using Dapper;
using Npgsql;

namespace DocuNest.Infrastructure.Persistence;

/// <summary>Every Dapper projection executes within a server-enforced read-only transaction.</summary>
public sealed class ReadOnlyProjectionSession : IAsyncDisposable
{
    private ReadOnlyProjectionSession(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        Connection = connection;
        Transaction = transaction;
    }

    public NpgsqlConnection Connection { get; }
    public NpgsqlTransaction Transaction { get; }

    public static async Task<ReadOnlyProjectionSession> OpenAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            await connection.ExecuteAsync(new CommandDefinition("SET TRANSACTION READ ONLY", transaction: transaction, cancellationToken: cancellationToken));
            await SearchPathGuardInterceptor.SelectAndVerifyAsync(connection, transaction, local: true, cancellationToken);
            return new ReadOnlyProjectionSession(connection, transaction);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Transaction.DisposeAsync();
        await Connection.DisposeAsync();
    }
}
