namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.TryLockStart,
        LogLevel.Debug,
        "Trying to acquire advisory lock '{id}'.")]
    public static partial void TryLockStart(this ILogger logger, long id);

    [LoggerMessage(
        (int)SloopEventId.TryLockAcquired,
        LogLevel.Debug,
        "Acquired advisory lock '{id}'.")]
    public static partial void TryLockAcquired(this ILogger logger, long id);

    [LoggerMessage(
        (int)SloopEventId.TryLockDenied,
        LogLevel.Debug,
        "Failed to acquire advisory lock '{id}'.")]
    public static partial void TryLockDenied(this ILogger logger, long id);
}