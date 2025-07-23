namespace Sloop.Commands;

using Abstractions;
using Core;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Arguments for removing a cache item by key.
/// </summary>
/// <param name="Key">The key of the cache item to remove.</param>
public record RemoveItemArgs(string Key);

/// <summary>
///     Command to delete a cache entry from PostgreSQL by key.
/// </summary>
public class RemoveItemCommand : IDbCacheCommand<RemoveItemArgs, bool>
{
    private readonly ILogger<RemoveItemCommand> _logger;

    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemoveItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public RemoveItemCommand(IOptions<SloopOptions> options, ILogger<RemoveItemCommand> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RemoveItemArgs args, CancellationToken token = default)
    {
        _logger.RemoveStart(args.Key);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             DELETE FROM {_options.GetQualifiedTableName()}
             WHERE key = @key;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);

        _logger.ExecutingSql(cmd.CommandText);

        var count = await cmd.ExecuteNonQueryAsync(token);

        if (count == 0)
        {
            _logger.RemoveNoop(args.Key);
        }
        else
        {
            _logger.RemoveDeleted(args.Key, count);
        }

        return count > 0;
    }
}