[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=coverage)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=dev-hancock_Sloop&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=dev-hancock_Sloop)

# ⚓ Sloop

**Sloop** is a lightweight, PostgreSQL-backed implementation of `IDistributedCache` for .NET applications. It provides durable, high-performance caching with support for sliding and absolute expiration. Designed for simplicity and resilience in distributed systems.

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
    options.DefaultExpiration = TimeSpan.FromMinutes(30); // optional
});
```

### Use the `IDistributedCache`

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

Tests are executed against disposable PostgreSQL containers via [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet):

```bash
dotnet test
```

## 🤝 Contributing

Contributions are welcome! Please follow standard C# coding guidelines and include tests with pull requests.

## 📄 License

Licensed under the [MIT License](LICENSE).


