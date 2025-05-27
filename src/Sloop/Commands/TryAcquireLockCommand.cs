using Npgsql;

namespace Sloop.Commands;

public record TryAcquireLockArgs(long Id);

public class TryAcquireLockCommand : IDbCacheCommand<TryAcquireLockArgs, bool>
{
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, TryAcquireLockArgs args, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT pg_try_advisory_lock(@id);";

        cmd.Parameters.AddWithValue("id", args.Id);

        var acquired = await cmd.ExecuteScalarAsync(token);

        return acquired is true; 
    }
}