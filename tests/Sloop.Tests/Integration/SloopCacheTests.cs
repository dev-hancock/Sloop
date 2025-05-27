namespace Sloop.Tests.Integration;

using System.Text;
using Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;

public class SloopCacheTests : IAsyncLifetime
{
    private PostgreSqlContainer _db = null!;

    private IDbConnectionFactory _connection = null!;

    private IDbCacheOperations _operations = null!;

    private const string Schema = "public";

    private const string Table = "cache";

    private static readonly TimeSpan DateTimeTolerance = TimeSpan.FromSeconds(1);

    public async Task InitializeAsync()
    {
        _db = new PostgreSqlBuilder()
            .WithDatabase("db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _db.StartAsync();

        var services = new ServiceCollection();

        services.AddCache(opt =>
        {
            opt.ConnectionString = _db.GetConnectionString();
            opt.SchemaName = Schema;
            opt.TableName = Table;
            opt.DefaultExpiration = null;
        });
        
        var provider = services.BuildServiceProvider();
        
        _operations = provider.GetRequiredService<IDbCacheOperations>();

        _connection = provider.GetRequiredService<IDbConnectionFactory>();
    }

    public async Task DisposeAsync()
    {
        await _db.StopAsync();
    }

    private static byte[] Serialize(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    private static string Deserialize(byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    private (DateTimeOffset? Expiry, TimeSpan? Sliding, DateTimeOffset? Absolute) GetMetadata(NpgsqlConnection connection, string key)
    {
        using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             SELECT expires_at, sliding_interval, absolute_expiry
             FROM {Schema}.{Table}
             WHERE key = @key;
             """;
        cmd.Parameters.AddWithValue("key", key);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return default;
        }

        return (
            reader.IsDBNull(0) ? null : reader.GetFieldValue<DateTimeOffset?>(0),
            reader.IsDBNull(1) ? null : reader.GetFieldValue<TimeSpan?>(1),
            reader.IsDBNull(2) ? null : reader.GetFieldValue<DateTimeOffset?>(2)
        );
    }

    [Fact]
    public async Task CreateTable_ShouldSucceed_AndTableShouldExist()
    {
        await using var connection = await _connection.Create();

        await using var cmd = connection.CreateCommand();
        
        cmd.CommandText =
            $"""
             SELECT EXISTS (
                 SELECT 1 FROM information_schema.tables
                 WHERE table_schema = '{Schema}'
                 AND table_name = '{Table}'
             );
             """;

        var exists = cmd.ExecuteScalar();

        Assert.True((bool)(exists ?? false));
    }

    [Fact]
    public async Task GetAsync_ShouldRefreshSliding_WhenEligible()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("refresh"),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMilliseconds(500)
                }));

        var before = GetMetadata(connection, key).Expiry;

        await Task.Delay(100);

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.NotNull(result);

        var after = GetMetadata(connection, key).Expiry;

        Assert.True(after > before);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnValue_AfterSetAsync()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        var value = Serialize("hello");

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                value,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                }));

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.NotNull(result);
        Assert.Equal("hello", Deserialize(result));
    }

    [Fact]
    public async Task PurgeExpired_ShouldRemoveEntry_WhenExpiresAtIsPast()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetAsync(connection,
            key,
            Serialize("expired"),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMilliseconds(1)
            });

        await Task.Delay(20);

        await _operations.PurgeExpired(connection);

        var result = await _operations.GetAsync(connection, key);

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ShouldExtendExpiration_WhenSlidingExpirationIsSet()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetAsync(connection,
            key,
            Serialize("data"),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(2)
            });

        await Task.Delay(1500);

        var refreshed = await _operations.RefreshAsync(connection, key);

        Assert.True(refreshed);

        var stillExists = await _operations.GetAsync(connection, key);
        Assert.NotNull(stillExists);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteEntry_WhenKeyExists()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetAsync(connection,
            key,
            Serialize("data"),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });

        await _operations.RemoveAsync(connection, key);

        var result = await _operations.GetAsync(connection, key);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldRespectAbsoluteExpiration_WhenSetExplicitly()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        var absolute = DateTimeOffset.UtcNow.AddSeconds(1);

        await _operations.SetAsync(connection,
            key,
            Serialize("absolute"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absolute
            });

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta.Expiry);
        Assert.Null(meta.Sliding);
        Assert.NotNull(meta.Absolute);

        await Task.Delay(2000);

        var result = await _operations.GetAsync(connection, key);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldSetExpiresAt_FromSlidingInterval_WhenNoAbsoluteExpiration()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetAsync(connection,
            key,
            Serialize("slide"),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta.Expiry);
        Assert.NotNull(meta.Sliding);
        Assert.Null(meta.Absolute);
    }

    [Fact]
    public async Task SetAsync_ShouldSetExpiresAt_ToMinimum_WhenBothSlidingAndAbsoluteSet()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        var absolute = DateTimeOffset.UtcNow.AddSeconds(5);

        await _operations.SetAsync(connection,
            key,
            Serialize("combo"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absolute,
                SlidingExpiration = TimeSpan.FromSeconds(60)
            });

        var meta = GetMetadata(connection, key);

        Assert.Equal(absolute, meta.Expiry ?? DateTimeOffset.MinValue, DateTimeTolerance);
        Assert.Equal(TimeSpan.FromSeconds(60), meta.Sliding);
        Assert.Equal(absolute, meta.Absolute ?? DateTimeOffset.MinValue, DateTimeTolerance);
    }

    [Fact]
    public async Task SetAsync_ShouldStorePermanentEntry_WhenNoExpirationProvided()
    {
        await using var connection = _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetAsync(connection,
            key,
            Serialize("permanent"),
            new DistributedCacheEntryOptions());

        var meta = GetMetadata(connection, key);

        Assert.Null(meta.Expiry);
        Assert.Null(meta.Sliding);
        Assert.Null(meta.Absolute);

        var result = await _operations.PurgeExpired(connection);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task TryAcquireLock_ShouldSucceedFirstCall_ThenFail_WhenAlreadyHeld()
    {
        await using var first = _connection.Create();

        const int id = 42_000;

        var success = await _operations.TryAcquireLock(first, id);

        Assert.True(success);

        await using var second = _connection.Create();

        var otherAttempt = await _operations.TryAcquireLock(second, id);

        Assert.False(otherAttempt);
    }
}