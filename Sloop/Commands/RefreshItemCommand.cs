namespace Sloop.Commands;

using Abstractions;
using Core;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Arguments for refreshing the expiration of a cached item.
/// </summary>
/// <param name="Key">The cache key to refresh.</param>
public record RefreshItemArgs(string Key);

/// <summary>
///     Command to update the expiration of a cache entry using sliding expiration policy.
/// </summary>
public class RefreshItemCommand : IDbCacheCommand<RefreshItemArgs, bool>
{
    private readonly ILogger<RefreshItemCommand> _logger;

    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public RefreshItemCommand(IOptions<SloopOptions> options, ILogger<RefreshItemCommand> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RefreshItemArgs args, CancellationToken token = default)
    {
        _logger.RefreshStart(args.Key);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE {_options.GetQualifiedTableName()}
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key AND sliding_interval IS NOT NULL;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);

        _logger.ExecutingSql(cmd.CommandText);

        var count = await cmd.ExecuteNonQueryAsync(token);

        if (count == 0)
        {
            _logger.RefreshNoop(args.Key);
        }
        else
        {
            _logger.RefreshUpdated(args.Key);
        }

        return count > 0;
    }
}