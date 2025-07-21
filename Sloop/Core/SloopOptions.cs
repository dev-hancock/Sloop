namespace Sloop.Core;

using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Represents configuration options for the PostgreSQL-based distributed cache.
/// </summary>
public class SloopOptions
{
    /// <summary>
    ///     The default absolute expiration interval for cache entries if none is explicitly specified.
    /// </summary>
    public TimeSpan? DefaultAbsoluteExpiration { get; set; }

    /// <summary>
    ///     The default sliding expiration interval for cache entries if none is explicitly specified.
    ///     Default is 20 minutes.
    /// </summary>
    public TimeSpan? DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    ///     The schema name where the cache table will be created and queried.
    ///     Default is "public"
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    ///     The name of the table used to store cache entries.
    ///     Default is "cache"
    /// </summary>
    public string TableName { get; set; } = "cache_items";

    /// <summary>
    ///     The interval at which the background cleanup service purges expired cache entries.
    ///     Defaults to 5 minutes.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Indicates if the infrastructure should be created if table doesn't exist
    /// </summary>
    public bool CreateInfrastructure { get; set; } = true;

    /// <summary>
    ///     Indicates whether schema qualification should be used in SQL.
    ///     True when <c>SchemaName</c> is set; false to rely on search_path.
    /// </summary>
    private bool UseSchemaQualification => !string.IsNullOrWhiteSpace(SchemaName);

    /// <summary>
    ///     The <see cref="NpgsqlDataSource" /> used to open connections.
    /// </summary>

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    /// <summary>
    ///     Returns the properly quoted table name identifier.
    /// </summary>
    /// <returns>The table name wrapped in double quotes, e.g. <c>"cache"</c>.</returns>
    internal string GetTableName()
    {
        return $"\"{TableName}\"";
    }

    /// <summary>
    ///     Gets the effective schema name: explicit <c>SchemaName</c> or the first search_path entry,
    ///     or "public" if neither is set.
    /// </summary>
    internal string GetEffectiveSchema()
    {
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            return SchemaName!;
        }

        var builder = new NpgsqlConnectionStringBuilder(DataSource.ConnectionString);

        if (!string.IsNullOrWhiteSpace(builder.SearchPath))
        {
            return builder.SearchPath.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        }

        return "public";
    }

    /// <summary>
    ///     Returns the properly quoted table identifier, with schema if <c>UseSchemaQualification</c> is true,
    ///     else just the quoted table name.
    /// </summary>
    internal string GetQualifiedTableName()
    {
        return UseSchemaQualification ? $"\"{GetEffectiveSchema()}\".\"{TableName}\"" : $"\"{TableName}\"";
    }

    /// <summary>
    ///     Configures the options to use a connection string and optional builder configuration.
    /// </summary>
    /// <param name="connectionString">The raw PostgreSQL connection string.</param>
    /// <param name="configure">
    ///     An optional callback to customize the <see cref="NpgsqlConnectionStringBuilder" /> before
    ///     building the data source.
    /// </param>
    public void UseConnectionString(string connectionString, Action<NpgsqlConnectionStringBuilder>? configure = null)
    {
        var build = new NpgsqlConnectionStringBuilder(connectionString);

        configure?.Invoke(build);

        DataSource = NpgsqlDataSource.Create(build.ConnectionString);
    }

    /// <summary>
    ///     Configures the options to use a connection string and optional data source builder configuration.
    /// </summary>
    /// <param name="connectionString">The raw PostgreSQL connection string.</param>
    /// <param name="configure">
    ///     An optional callback to customize the <see cref="NpgsqlDataSourceBuilder" /> before building the data source.
    /// </param>
    public void UseDataSource(string connectionString, Action<NpgsqlDataSourceBuilder>? configure)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);

        configure?.Invoke(builder);

        DataSource = builder.Build();
    }

    /// <summary>
    ///     Configures the options to use an existing <see cref="NpgsqlDataSource" />.
    /// </summary>
    /// <param name="dataSource">The pre‐built <see cref="NpgsqlDataSource" />.</param>
    public void UseDataSource(NpgsqlDataSource dataSource)
    {
        DataSource = dataSource;
    }

    /// <summary>
    ///     Validates the current <see cref="SloopOptions" /> values and throws an <see cref="OptionsValidationException" />
    ///     if any required setting is missing or invalid.
    /// </summary>
    internal bool Validate()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "TableName must be provided."
                });
        }

        if (DefaultSlidingExpiration is { TotalSeconds: <= 0 })
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "DefaultExpiration must be a positive TimeSpan."
                });
        }

        if (DefaultAbsoluteExpiration is { TotalSeconds: <= 0 })
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "DefaultExpiration must be a positive TimeSpan."
                });
        }

        if (CleanupInterval is { TotalSeconds: <= 0 })
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "CleanupInterval must be a positive TimeSpan if specified."
                });
        }

        if (DataSource is null)
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "DataSource must not be null."
                });
        }

        return true;
    }
}