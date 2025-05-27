namespace Sloop
{
    public interface IDbCommandResolver
    {
        IDbCacheCommand<TArgs, TResult> Resolve<TArgs, TResult>();
    }
}