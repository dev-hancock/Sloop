namespace Sloop.Abstractions;

using Npgsql;

/// <summary>
///     Abstraction for creating new PostgreSQL connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    ///     Creates and returns a new open <see cref="NpgsqlConnection" />.
    /// </summary>
    Task<NpgsqlConnection> Create(CancellationToken ct = default);
}