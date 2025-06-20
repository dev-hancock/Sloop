namespace Sloop;

using Npgsql;

/// <summary>
///     Represents configuration options for the PostgreSQL-based distributed cache.
/// </summary>
public class SloopOptions
{
    /// <summary>
    ///     The connection string used to connect to the PostgreSQL instance.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    ///     A factory delegate used to create and configure <see cref="NpgsqlConnection" /> instances.
    /// </summary>
    public Func<string, NpgsqlConnection> ConnectionFactory { get; set; } = x => new NpgsqlConnection(x);

    /// <summary>
    ///     The default expiration interval for cache entries if none is explicitly specified.
    ///     Default is 20 minutes.
    /// </summary>
    public TimeSpan? DefaultExpiration { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    ///     The schema name where the cache table will be created and queried.
    ///     Default is "public"
    /// </summary>
    public string SchemaName { get; set; } = "public";

    /// <summary>
    ///     The name of the table used to store cache entries.
    ///     Default is "cache"
    /// </summary>
    public string TableName { get; set; } = "cache";

    /// <summary>
    ///     The interval at which the background cleanup service purges expired cache entries.
    ///     Defaults to 5 minutes.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
}