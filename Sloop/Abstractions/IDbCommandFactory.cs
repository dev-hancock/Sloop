namespace Sloop.Abstractions;

/// <summary>
///     Resolves concrete implementations of database cache commands.
///     Useful for generic dispatch or composition.
/// </summary>
public interface IDbCommandFactory
{
    /// <summary>
    ///     Resolves a command implementation for the given input and result types.
    /// </summary>
    /// <typeparam name="TArgs">The type of the command arguments.</typeparam>
    /// <typeparam name="TResult">The type of the command result.</typeparam>
    /// <returns>A concrete implementation of the database cache command.</returns>
    IDbCacheCommand<TArgs, TResult> Resolve<TArgs, TResult>();
}