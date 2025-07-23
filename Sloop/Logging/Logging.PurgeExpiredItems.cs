namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.PurgeStart,
        LogLevel.Debug,
        "Starting purge of expired items (limit={limit}).")]
    public static partial void PurgeStart(this ILogger logger, long limit);

    [LoggerMessage(
        (int)SloopEventId.PurgeBatchDone,
        LogLevel.Trace,
        "Deleted {count} expired items in this batch.")]
    public static partial void PurgeBatch(this ILogger logger, long count);

    [LoggerMessage(
        (int)SloopEventId.PurgeFinished,
        LogLevel.Information,
        "Finished purge. Total deleted={total}.")]
    public static partial void PurgeFinished(this ILogger logger, long total);

    [LoggerMessage(
        (int)SloopEventId.PurgeCancelled,
        LogLevel.Debug,
        "Cancellation requested. Total deleted so far={total}.")]
    public static partial void PurgeCancelled(this ILogger logger, long total);
}