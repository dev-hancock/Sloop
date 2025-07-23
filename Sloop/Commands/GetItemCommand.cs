namespace Sloop.Commands;

using Abstractions;
using Core;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Arguments required to retrieve an item by key.
/// </summary>
/// <param name="Key">The cache key.</param>
public record GetItemArgs(string Key);

/// <summary>
///     Command to retrieve a cached item from PostgreSQL.
///     Updates sliding expiration if applicable.
/// </summary>
public class GetItemCommand : IDbCacheCommand<GetItemArgs, byte[]?>
{
    private readonly ILogger<GetItemCommand> _logger;

    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public GetItemCommand(IOptions<SloopOptions> options, ILogger<GetItemCommand> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<byte[]?> ExecuteAsync(NpgsqlConnection connection, GetItemArgs args, CancellationToken token = default)
    {
        _logger.GetStart(args.Key);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE {_options.GetQualifiedTableName()}
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key
               AND (expires_at IS NULL OR expires_at > now())
               AND sliding_interval IS NOT NULL
             RETURNING value;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);

        _logger.ExecutingSql(cmd.CommandText);

        var result = await cmd.ExecuteScalarAsync(token);

        if (result is not byte[] value)
        {
            _logger.CacheMiss(args.Key);

            return null;
        }

        _logger.CacheHit(args.Key, value?.Length ?? 0);

        return value;
    }
}