namespace Sloop.Factories;

using Abstractions;
using Core;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Provides a thread-safe implementation of <see cref="IDbConnectionFactory" /> that uses
///     the configured <see cref="SloopOptions" /> to open pooled connections.
/// </summary>
public class SloopConnectionFactory : IDbConnectionFactory
{
    private readonly SloopOptions _options;

    /// <summary>
    ///     Constructs a new instance of <see cref="SloopConnectionFactory" />.
    /// </summary>
    /// <param name="options">The configured <see cref="SloopOptions" />.</param>
    public SloopConnectionFactory(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    ///     Creates and opens a new <see cref="NpgsqlConnection" /> from the configured data source.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken" /> to cancel the operation.</param>
    public async Task<NpgsqlConnection> Create(CancellationToken ct = default)
    {
        return await _options.DataSource.OpenConnectionAsync(ct).ConfigureAwait(false);
    }
}