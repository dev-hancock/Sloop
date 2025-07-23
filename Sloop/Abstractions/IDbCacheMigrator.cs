namespace Sloop.Abstractions;

/// <summary>Runs database migrations for the Sloop cache infrastructure.</summary>
public interface IDbCacheMigrator
{
    /// <summary>
    ///     Applies required schema/table updates for the cache store.
    /// </summary>
    /// <param name="ct">Token to cancel the migration.</param>
    Task MigrateAsync(CancellationToken ct = default);
}