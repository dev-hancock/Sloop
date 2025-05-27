using Npgsql;

namespace Sloop;


public interface IDbCacheCommand<in T>
{
    Task ExecuteAsync(NpgsqlConnection connection, T args, CancellationToken token = default);
}

public interface IDbCacheCommand<in TIn, TOut>
{
    Task<TOut> ExecuteAsync(NpgsqlConnection connection, TIn args, CancellationToken token = default);
}