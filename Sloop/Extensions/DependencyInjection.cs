namespace Sloop.Extensions;

using Abstractions;
using Commands;
using Core;
using Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Services;

/// <summary>
///     Provides extension methods for registering Sloop PostgreSQL cache components into a service collection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds Sloop's PostgreSQL-based <see cref="IDistributedCache" /> implementation and related services.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">A delegate to configure <see cref="SloopOptions" />.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCache(this IServiceCollection services, Action<SloopOptions> configure)
    {
        services
            .AddOptions<SloopOptions>()
            .Configure(configure)
            .Validate(x => x.Validate());

        services.AddSingleton(TimeProvider.System);

        services.AddHostedService<SloopMigrationService>();
        services.AddHostedService<SloopCleanupService>();

        services.AddTransient<IDbCacheCommand<GetItemArgs, byte[]?>, GetItemCommand>();
        services.AddTransient<IDbCacheCommand<PurgeExpiredItemsArgs, long>, PurgeExpiredItemsCommand>();
        services.AddTransient<IDbCacheCommand<RefreshItemArgs, bool>, RefreshItemCommand>();
        services.AddTransient<IDbCacheCommand<RemoveItemArgs, bool>, RemoveItemCommand>();
        services.AddTransient<IDbCacheCommand<SetItemArgs, bool>, SetItemCommand>();
        services.AddTransient<IDbCacheCommand<TryAcquireLockArgs, bool>, TryAcquireLockCommand>();

        services.AddSingleton<IDbCacheContext, SloopCacheContext>();
        services.AddSingleton<IDbCacheMigrator, SloopCacheMigrator>();
        services.AddSingleton<IDbCommandFactory, SloopCommandFactory>();
        services.AddSingleton<IDbCacheOperations, SloopOperations>();
        services.AddSingleton<IDbConnectionFactory, SloopConnectionFactory>();
        services.AddSingleton<IDistributedCache, SloopCache>();

        return services;
    }
}