namespace Sloop;

using Microsoft.Extensions.Options;

/// <summary>
///     Validates <see cref="SloopOptions" /> and throws exceptions on failure.
///     Suitable for small libraries where early failure is preferred.
/// </summary>
internal class SloopOptionsValidator : IConfigureOptions<SloopOptions>
{
    /// <inheritdoc />
    public void Configure(SloopOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "ConnectionString must be provided."
                });
        }

        if (string.IsNullOrWhiteSpace(options.SchemaName))
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "SchemaName must be provided."
                });
        }

        if (string.IsNullOrWhiteSpace(options.TableName))
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "TableName must be provided."
                });
        }

        if (options.DefaultExpiration is { TotalSeconds: <= 0 })
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "DefaultExpiration must be a positive TimeSpan."
                });
        }

        if (options.CleanupInterval is { TotalSeconds: <= 0 })
        {
            throw new OptionsValidationException(nameof(SloopOptions),
                typeof(SloopOptions),
                new[]
                {
                    "CleanupInterval must be a positive TimeSpan if specified."
                });
        }
    }
}