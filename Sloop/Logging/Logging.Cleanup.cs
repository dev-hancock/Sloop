namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        0,
        LogLevel.Information,
        "Sloop cleanup service started. Interval={interval}.")]
    public static partial void CleanupStarted(this ILogger logger, TimeSpan interval);

    [LoggerMessage(
        0,
        LogLevel.Information,
        "Sloop cleanup service stopping.")]
    public static partial void CleanupStopping(this ILogger logger);

    [LoggerMessage(
        0,
        LogLevel.Error,
        "Cleanup tick failed.")]
    public static partial void CleanupFailed(this ILogger logger, Exception exception);
}