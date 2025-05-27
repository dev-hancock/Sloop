namespace Sloop;

/// <summary>
/// Represents configuration options for the PostgreSQL-based distributed cache.
/// </summary>
public class SloopOptions
{
    /// <summary>
    /// The connection string used to connect to the PostgreSQL instance.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The default expiration interval for cache entries if none is explicitly specified.
    /// </summary>
    public TimeSpan? DefaultExpiration { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    /// The schema name where the cache table will be created and queried.
    /// </summary>
    public string SchemaName { get; set; } = "public";

    /// <summary>
    /// The name of the table used to store cache entries.
    /// </summary>
    public string TableName { get; set; } = "cache";
}