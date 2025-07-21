# Sloop

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=coverage)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)

![Sloop logo](https://raw.githubusercontent.com/dev-hancock/Sloop/main/icon.png)

**Sloop** is a lightweight, PostgreSQL-backed implementation of `IDistributedCache` for .NET applications. It provides
durable, high-performance caching with support for sliding and absolute expiration. Designed for simplicity and
resilience in distributed systems.

## ✨ Features

- ⚡ Distributed key-value store backed by PostgreSQL
- 🕓 Support for sliding and absolute expiration
- 🔁 Safe for multi-instance use
- 🧹 Periodic cleanup of expired entries
- 🧪 Fully integration-tested with high coverage
- 📦 Compatible with `Microsoft.Extensions.Caching.Abstractions`
- 🐘 Powered by Npgsql and Dapper
- 💉 Simple, idiomatic DI registration

## 📦 Installation

```bash
dotnet add package Sloop
````

> Requires **.NET 8.0 SDK or later**

## 🚀 Quick Start

### Register with Dependency Injection

```csharp
builder.Services.AddCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Postgres")!;
    options.SchemaName = "public";
    options.TableName = "cache";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
    options.CleanupInterval = TimeSpan.FromMinutes(1);
    options.ConnectionFactory = x => new NpgsqlConnection(x);
});
```

### Use `IDistributedCache`

```csharp
public class MyService
{
    private readonly IDistributedCache _cache;

    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task UseCacheAsync()
    {
        var key = "user:123";
        var data = await _cache.GetStringAsync(key);

        if (data == null)
        {
            data = "example";
            await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
    }
}
```

## 🧪 Testing

Tests are executed against disposable PostgreSQL containers
via [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet):

```bash
dotnet test
```

## 🏎 Benchmark Results

Tested on: **Intel i5-14600K, Arch Linux, .NET 8.0**

| Method              | Parallelism | Mean       | Allocated |
|---------------------|-------------|------------|-----------|
| `SetAsync`          | 1           | 60.05 µs   | 5.81 KB   |
| `GetAsync`          | 1           | 73.15 µs   | 3.76 KB   |
| `SetAsync_Parallel` | 1           | 60.00 µs   | 6.01 KB   |
| `GetAsync_Parallel` | 1           | 66.78 µs   | 4.14 KB   |
| `SetAsync`          | 10          | 60.10 µs   | 5.81 KB   |
| `GetAsync`          | 10          | 64.30 µs   | 3.76 KB   |
| `SetAsync_Parallel` | 10          | 580.55 µs  | 56.08 KB  |
| `GetAsync_Parallel` | 10          | 840.78 µs  | 36.81 KB  |
| `SetAsync`          | 50          | 61.44 µs   | 5.81 KB   |
| `GetAsync`          | 50          | 64.80 µs   | 3.76 KB   |
| `SetAsync_Parallel` | 50          | 872.56 µs  | 278.38 KB |
| `GetAsync_Parallel` | 50          | 2029.80 µs | 181.87 KB |

> Benchmarks run using [BenchmarkDotNet](https://benchmarkdotnet.org) on .NET 8.0 with Testcontainers for PostgreSQL.

## 🤝 Contributing

Contributions are welcome! Please follow standard C# coding guidelines and include tests with pull requests.

## 📄 License

Licensed under the [MIT License](LICENSE).
