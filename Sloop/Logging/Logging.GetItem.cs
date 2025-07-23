namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.GetItemStart,
        LogLevel.Debug,
        "Retrieving cache item '{key}'.")]
    public static partial void GetStart(this ILogger logger, string key);

    [LoggerMessage(
        (int)SloopEventId.GetItemHit,
        LogLevel.Debug,
        "Cache hit for key '{key}' ({length} bytes).")]
    public static partial void CacheHit(this ILogger logger, string key, long length);

    [LoggerMessage(
        (int)SloopEventId.GetItemMiss,
        LogLevel.Debug,
        "Cache miss for key '{key}'.")]
    public static partial void CacheMiss(this ILogger logger, string key);
}