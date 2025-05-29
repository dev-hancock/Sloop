namespace Sloop.Commands;

using Interfaces;
using Npgsql;

/// <summary>
///     Arguments for attempting to acquire a PostgreSQL advisory lock.
/// </summary>
/// <param name="Id">The lock identifier.</param>
public record TryAcquireLockArgs(long Id);

/// <summary>
///     Command to attempt acquiring a PostgreSQL advisory lock using a numeric key.
///     Useful for preventing concurrent cache operations across distributed nodes.
/// </summary>
public class TryAcquireLockCommand : IDbCacheCommand<TryAcquireLockArgs, bool>
{
    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, TryAcquireLockArgs args, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT pg_try_advisory_lock(@id);";

        cmd.Parameters.AddWithValue("id", args.Id);

        var acquired = await cmd.ExecuteScalarAsync(token);

        return acquired is true;
    }
}