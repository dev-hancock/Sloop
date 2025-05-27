namespace Sloop;

using Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Resolves instances of <see cref="IDbCacheCommand{TIn,TOut}" /> from the dependency injection container.
///     Enables runtime resolution of generic cache commands using reflection-free DI.
/// </summary>
public class SloopCommandResolver : IDbCommandResolver
{
    private readonly IServiceProvider _services;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopCommandResolver" /> class.
    /// </summary>
    /// <param name="services">The root <see cref="IServiceProvider" /> used to resolve command implementations.</param>
    public SloopCommandResolver(IServiceProvider services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public IDbCacheCommand<TArgs, TResult> Resolve<TArgs, TResult>()
    {
        return _services.GetRequiredService<IDbCacheCommand<TArgs, TResult>>();
    }
}