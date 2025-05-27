namespace Sloop.Commands;

using Interfaces;
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
    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RemoveItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    public RemoveItemCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RemoveItemArgs args, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             DELETE FROM "{_options.SchemaName}"."{_options.TableName}"
             WHERE key = @key;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);

        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }
}