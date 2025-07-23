namespace Sloop.Tests.Integration;

using System.Text;
using Abstractions;
using Commands;
using Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Testcontainers.PostgreSql;

public class SloopCacheTests : IAsyncLifetime
{
    private const string Schema = "public";

    private const string Table = "cache";

    private static readonly TimeSpan DateTimeTolerance = TimeSpan.FromSeconds(1);

    private IDbConnectionFactory _connection = null!;

    private IDbCacheContext _context = null!;

    private PostgreSqlContainer _db = null!;

    private IDbCacheOperations _operations = null!;

    public async Task InitializeAsync()
    {
        _db = new PostgreSqlBuilder()
            .WithDatabase("db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _db.StartAsync();

        var services = new ServiceCollection();

        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        services.AddCache(opt =>
        {
            opt.UseConnectionString(_db.GetConnectionString());
            opt.SchemaName = Schema;
            opt.TableName = Table;
            opt.DefaultSlidingExpiration = null;
            opt.CreateInfrastructure = true;
        });

        var provider = services.BuildServiceProvider();

        _operations = provider.GetRequiredService<IDbCacheOperations>();

        _connection = provider.GetRequiredService<IDbConnectionFactory>();

        _context = provider.GetRequiredService<IDbCacheContext>();

        await _context.MigrateAsync();
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

    private (DateTimeOffset? Expiry, TimeSpan? Sliding, DateTimeOffset? Absolute)? GetMetadata(NpgsqlConnection connection,
        string key)
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
            return null;
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

        var before = GetMetadata(connection, key)?.Expiry;

        await Task.Delay(100);

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.NotNull(result);

        var after = GetMetadata(connection, key)?.Expiry;

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
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("expired"),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMilliseconds(1)
                }));

        await Task.Delay(20);

        await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs());

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ShouldExtendExpiration_WhenSlidingExpirationIsSet()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("data"),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(2)
                }));

        await Task.Delay(1500);

        var refreshed = await _operations.RefreshItem.ExecuteAsync(connection, new RefreshItemArgs(key));

        Assert.True(refreshed);

        var exists = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.NotNull(exists);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteEntry_WhenKeyExists()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("data"),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                }));

        await _operations.RemoveItem.ExecuteAsync(connection, new RemoveItemArgs(key));

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldRespectAbsoluteExpiration_WhenSetExplicitly()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        var absolute = DateTimeOffset.UtcNow.AddSeconds(1);

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("absolute"),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = absolute
                }));

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta);
        Assert.NotNull(meta.Value.Expiry);
        Assert.Null(meta.Value.Sliding);
        Assert.NotNull(meta.Value.Absolute);

        await Task.Delay(2000);

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldSetExpiresAt_FromSlidingInterval_WhenNoAbsoluteExpiration()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("slide"),
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(5)
                }));

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta);
        Assert.NotNull(meta.Value.Expiry);
        Assert.NotNull(meta.Value.Sliding);
        Assert.Null(meta.Value.Absolute);
    }

    [Fact]
    public async Task SetAsync_ShouldSetExpiresAt_ToMinimum_WhenBothSlidingAndAbsoluteSet()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        var absolute = DateTimeOffset.UtcNow.AddSeconds(5);

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("combo"),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = absolute,
                    SlidingExpiration = TimeSpan.FromSeconds(60)
                }));

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta);
        Assert.Equal(absolute, meta.Value.Expiry ?? DateTimeOffset.MinValue, DateTimeTolerance);
        Assert.Equal(TimeSpan.FromSeconds(60), meta.Value.Sliding);
        Assert.Equal(absolute, meta.Value.Absolute ?? DateTimeOffset.MinValue, DateTimeTolerance);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreNullValue_WhenValueIsNull()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(
            connection,
            new SetItemArgs(key, null!, new DistributedCacheEntryOptions())
        );

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenStoredValueIsNull()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(
            connection,
            new SetItemArgs(key, null!, new DistributedCacheEntryOptions())
        );

        var result = await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key));

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldStorePermanentEntry_WhenNoExpirationProvided()
    {
        await using var connection = await _connection.Create();

        var key = Guid.NewGuid().ToString();

        await _operations.SetItem.ExecuteAsync(connection,
            new SetItemArgs(key,
                Serialize("permanent"),
                new DistributedCacheEntryOptions()));

        var meta = GetMetadata(connection, key);

        Assert.NotNull(meta);
        Assert.Null(meta.Value.Expiry);
        Assert.Null(meta.Value.Sliding);
        Assert.Null(meta.Value.Absolute);

        var result = await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs());

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task TryAcquireLock_ShouldSucceedFirstCall_ThenFail_WhenAlreadyHeld()
    {
        await using var first = await _connection.Create();

        var id = new TryAcquireLockArgs(42_000);

        var success = await _operations.TryAcquireLock.ExecuteAsync(first, id);

        Assert.True(success);

        await using var second = await _connection.Create();

        var otherAttempt = await _operations.TryAcquireLock.ExecuteAsync(second, id);

        Assert.False(otherAttempt);
    }
}