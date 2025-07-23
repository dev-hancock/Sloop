namespace Sloop.Logging;

using Microsoft.Extensions.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(
        (int)SloopEventId.SkippingMigration,
        LogLevel.Debug,
        "CreateInfrastructure is false. Skipping cache migration.")]
    public static partial void SkippingMigration(this ILogger logger);

    [LoggerMessage(
        (int)SloopEventId.StartingMigration,
        LogLevel.Information,
        "Applying cache migration ('{table}').")]
    public static partial void StartingMigration(this ILogger logger, string table);

    [LoggerMessage(
        (int)SloopEventId.MigrationDone,
        LogLevel.Information,
        "Cache migration completed.")]
    public static partial void MigrationDone(this ILogger logger);

    [LoggerMessage(
        0,
        LogLevel.Error,
        "Cache migration failed.")]
    public static partial void MigrationFailed(this ILogger logger, Exception exception);
}