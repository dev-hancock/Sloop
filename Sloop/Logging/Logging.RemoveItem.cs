namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.RemoveStart,
        LogLevel.Debug,
        "Removing cache item '{key}'.")]
    public static partial void RemoveStart(this ILogger logger, string key);

    [LoggerMessage(
        (int)SloopEventId.RemoveDeleted,
        LogLevel.Debug,
        "Removed cache item '{key}'. Rows affected={count}.")]
    public static partial void RemoveDeleted(this ILogger logger, string key, int count);

    [LoggerMessage(
        (int)SloopEventId.RemoveNoop,
        LogLevel.Debug,
        "No cache item removed for key '{key}'.")]
    public static partial void RemoveNoop(this ILogger logger, string key);
}