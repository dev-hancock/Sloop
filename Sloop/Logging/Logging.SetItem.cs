namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.SetStart,
        LogLevel.Debug,
        "Setting cache item '{key}' ({length} bytes).")]
    public static partial void SetStart(this ILogger logger, string key, int length);

    [LoggerMessage(
        (int)SloopEventId.SetStored,
        LogLevel.Debug,
        "Cache item '{key}' stored (upsert).")]
    public static partial void SetStored(this ILogger logger, string key);

    [LoggerMessage(
        (int)SloopEventId.SetNoop,
        LogLevel.Debug,
        "No row affected for key '{key}'.")]
    public static partial void SetNoop(this ILogger logger, string key);
}