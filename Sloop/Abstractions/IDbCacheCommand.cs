namespace Sloop.Abstractions;

using Npgsql;

/// <summary>
///     Represents a database-backed cache command that can be executed against a PostgreSQL connection.
/// </summary>
/// <typeparam name="TIn">The type of the input argument.</typeparam>
/// <typeparam name="TOut">The type of the result returned by the command.</typeparam>
public interface IDbCacheCommand<in TIn, TOut>
{
    /// <summary>
    ///     Executes the command using the provided connection and input arguments.
    /// </summary>
    /// <param name="connection">An open PostgreSQL connection.</param>
    /// <param name="args">The input arguments for the command.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The result of the command execution.</returns>
    Task<TOut> ExecuteAsync(NpgsqlConnection connection, TIn args, CancellationToken ct = default);
}