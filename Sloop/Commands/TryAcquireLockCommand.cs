namespace Sloop.Commands;

using Abstractions;
using Logging;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TryAcquireLockCommand> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TryAcquireLockCommand" /> class.
    /// </summary>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public TryAcquireLockCommand(ILogger<TryAcquireLockCommand> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, TryAcquireLockArgs args, CancellationToken token = default)
    {
        _logger.TryLockStart(args.Id);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT pg_try_advisory_lock(@id);";

        cmd.Parameters.AddWithValue("id", args.Id);

        _logger.ExecutingSql(cmd.CommandText);

        var result = await cmd.ExecuteScalarAsync(token);

        var acquired = result is true;

        if (acquired)
        {
            _logger.TryLockAcquired(args.Id);
        }
        else
        {
            _logger.TryLockDenied(args.Id);
        }

        return acquired;
    }
}