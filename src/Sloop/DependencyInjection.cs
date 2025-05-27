namespace Sloop;

using Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services, Action<SloopOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton(TimeProvider.System);

        services.AddHostedService<SloopCleanupService>();

        services.AddTransient<IDbCacheCommand<CreateTableArgs, bool>>();
        services.AddTransient<IDbCacheCommand<GetItemArgs, byte[]?>>();
        services.AddTransient<IDbCacheCommand<PurgeExpiredItemsArgs, long>>();
        services.AddTransient<IDbCacheCommand<RefreshItemArgs, bool>>();
        services.AddTransient<IDbCacheCommand<RemoveItemArgs, bool>>();
        services.AddTransient<IDbCacheCommand<SetItemArgs, bool>>();
        services.AddTransient<IDbCacheCommand<TryAcquireLockArgs, bool>>();

        services.AddTransient<IDbCommandResolver, SloopCommandResolver>();
        services.AddSingleton<IDbCacheOperations, SloopOperations>();
        services.AddSingleton<IDbConnectionFactory, SloopConnectionFactory>();
        
        services.AddSingleton<IDistributedCache, SloopCache>();

        services.AddSingleton<SloopServices>();

        return services;
    }
}