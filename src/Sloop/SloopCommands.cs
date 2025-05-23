namespace Sloop;

using Npgsql;

/// <summary>
/// Provides raw Npgsql SQL command builders for distributed cache operations.
/// </summary>
public static class SloopCommands
{
    /// <summary>
    /// Returns a command that creates the cache schema and table if they do not already exist.
    /// The table includes support for sliding expiration and automatic expiration via index.
    /// </summary>
    public static NpgsqlCommand CreateTable(NpgsqlConnection connection, string schema, string table)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             CREATE SCHEMA IF NOT EXISTS "{schema}";

             CREATE UNLOGGED TABLE IF NOT EXISTS "{schema}"."{table}" (
                key TEXT NOT NULL PRIMARY KEY,
                value BYTEA NOT NULL,
                expires_at TIMESTAMPTZ NULL,
                sliding_interval INTERVAL NULL,
                absolute_expiry TIMESTAMPTZ NULL
             );

             CREATE INDEX IF NOT EXISTS "{table}_expires_at"
             ON "{schema}"."{table}" (expires_at);
             """;

        return cmd;
    }

    /// <summary>
    /// Returns a command that selects the cached value by key, if not expired.
    /// </summary>
    public static NpgsqlCommand GetItem(NpgsqlConnection connection, string schema, string table, string key)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE "{schema}"."{table}"
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key
               AND (expires_at IS NULL OR expires_at > now())
               AND sliding_interval IS NOT NULL
             RETURNING value;
             """;

        cmd.Parameters.AddWithValue("key", key);

        return cmd;
    }

    /// <summary>
    /// Returns a command that deletes up to 1000 expired cache entries in a single batch.
    /// Intended to be called repeatedly in a loop until no more entries are found.
    /// </summary>
    public static NpgsqlCommand PurgeExpired(NpgsqlConnection connection, string schema, string table)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             DELETE FROM "{schema}"."{table}"
             WHERE ctid IN (
                 SELECT ctid FROM "{schema}"."{table}"
                 WHERE expires_at <= now()
                 LIMIT 1000
             );
             """;

        return cmd;
    }

    /// <summary>
    /// Returns a command that refreshes the expiration of an existing cache entry
    /// using the stored sliding interval. Only applies if sliding_expiry is not null.
    /// </summary>
    public static NpgsqlCommand RefreshItem(NpgsqlConnection connection, string schema, string table, string key)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE "{schema}"."{table}"
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key AND sliding_interval IS NOT NULL;
             """;

        cmd.Parameters.AddWithValue("key", key);

        return cmd;
    }

    /// <summary>
    /// Returns a command that deletes a cache entry by key.
    /// </summary>
    public static NpgsqlCommand RemoveItem(NpgsqlConnection connection, string schema, string table, string key)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             DELETE FROM "{schema}"."{table}"
             WHERE key = @key;
             """;

        cmd.Parameters.AddWithValue("key", key);

        return cmd;
    }

    /// <summary>
    /// Returns a command that inserts or updates a cache entry with value and expiration metadata.
    /// Supports absolute expiration, sliding expiration, both, or none (non-expiring).
    /// </summary>
    public static NpgsqlCommand SetItem(NpgsqlConnection connection, string schema, string table, string key, byte[] value, DateTimeOffset? expiry, TimeSpan? sliding, DateTimeOffset? absolute)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             INSERT INTO "{schema}"."{table}" (key, value, expires_at, sliding_interval, absolute_expiry)
             VALUES (@key, @value, @expires_at, @sliding_interval, @absolute_expiry)
             ON CONFLICT (key) DO UPDATE 
             SET value = @value,
                 expires_at = @expires_at,
                 sliding_interval = @sliding_interval,
                 absolute_expiry = @absolute_expiry;
             """;

        cmd.Parameters.AddWithValue("key", key);
        cmd.Parameters.AddWithValue("value", value);
        cmd.Parameters.AddWithValue("expires_at", expiry.HasValue ? expiry.Value.UtcDateTime : DBNull.Value);
        cmd.Parameters.AddWithValue("sliding_interval", sliding.HasValue ? sliding.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("absolute_expiry", absolute.HasValue ? absolute.Value.UtcDateTime : DBNull.Value);

        return cmd;
    }

    /// <summary>
    /// Returns a command that attempts to acquire a PostgreSQL advisory lock for coordination across instances.
    /// </summary>
    public static NpgsqlCommand TryAcquireLock(NpgsqlConnection connection, long id)
    {
        var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT pg_try_advisory_lock(@id);";

        cmd.Parameters.AddWithValue("id", id);

        return cmd;
    }
}