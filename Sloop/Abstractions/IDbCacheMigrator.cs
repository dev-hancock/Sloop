namespace Sloop.Abstractions;

public interface IDbCacheMigrator
{
    Task MigrateAsync(CancellationToken ct = default);
}