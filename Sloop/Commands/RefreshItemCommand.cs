namespace Sloop.Commands;

using Abstractions;
using Core;
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
    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    public RefreshItemCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RefreshItemArgs itemArgs, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE {_options.GetQualifiedTableName()}
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key AND sliding_interval IS NOT NULL;
             """;

        cmd.Parameters.AddWithValue("key", itemArgs.Key);

        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }
}