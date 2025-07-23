namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.ExecutingSql,
        LogLevel.Trace,
        "Executing SQL for cache migration:\n{sql}")]
    public static partial void ExecutingSql(this ILogger logger, string sql);
}