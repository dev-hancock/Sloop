namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.RefreshStart,
        LogLevel.Debug,
        "Refreshing cache item '{key}'.")]
    public static partial void RefreshStart(this ILogger logger, string key);

    [LoggerMessage(
        (int)SloopEventId.RefreshUpdated,
        LogLevel.Debug,
        "Expiration refreshed for key '{key}'.")]
    public static partial void RefreshUpdated(this ILogger logger, string key);

    [LoggerMessage(
        (int)SloopEventId.RefreshNoop,
        LogLevel.Debug,
        "No refresh performed for key '{key}'.")]
    public static partial void RefreshNoop(this ILogger logger, string key);
}