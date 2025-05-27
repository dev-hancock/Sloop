namespace Sloop;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services, Action<SloopOptions> configure)
    {
        services.Configure(configure);

        services.AddHostedService<SloopCleanupService>();

        services.AddSingleton(TimeProvider.System);
        
        

        services.AddSingleton<IDbCacheOperations, SloopOperations>();
        services.AddSingleton<IDbConnectionFactory, SloopConnectionFactory>();
        services.AddSingleton<IDistributedCache, SloopCache>();

        services.AddSingleton<SloopServices>();

        return services;
    }
}