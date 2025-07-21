namespace Sloop.Tests.Unit;

using Core;
using Microsoft.Extensions.Options;

public class SloopOptionsValidatorTests
{
    [Fact]
    public void Configure_ShouldThrow_WhenTableNameIsNull()
    {
        var options = new SloopOptions
        {
            TableName = null!
        };

        options.UseConnectionString("Host=localhost;");

        var ex = Assert.Throws<OptionsValidationException>(() => options.Validate());

        Assert.Contains("TableName must be provided.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenDefaultSlidingExpirationIsNonPositive()
    {
        var options = new SloopOptions
        {
            DefaultSlidingExpiration = TimeSpan.Zero
        };

        options.UseConnectionString("Host=localhost;");

        var ex = Assert.Throws<OptionsValidationException>(() => options.Validate());

        Assert.Contains("DefaultExpiration must be a positive TimeSpan.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenDefaultAbsoluteExpirationIsNonPositive()
    {
        var options = new SloopOptions
        {
            DefaultAbsoluteExpiration = TimeSpan.Zero
        };

        options.UseConnectionString("Host=localhost;");

        var ex = Assert.Throws<OptionsValidationException>(() => options.Validate());

        Assert.Contains("DefaultExpiration must be a positive TimeSpan.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenCleanupIntervalIsNonPositive()
    {
        var options = new SloopOptions
        {
            CleanupInterval = TimeSpan.Zero
        };

        options.UseConnectionString("Host=localhost;");

        var ex = Assert.Throws<OptionsValidationException>(() => options.Validate());

        Assert.Contains("CleanupInterval must be a positive TimeSpan if specified.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldSucceed_WhenOptionsAreValid()
    {
        var options = new SloopOptions();

        options.UseConnectionString("Host=localhost;");

        var exception = Record.Exception(() => options.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenDataSourceIsNull()
    {
        var options = new SloopOptions();

        var ex = Assert.Throws<OptionsValidationException>(() => options.Validate());

        Assert.Contains("DataSource must not be null.", ex.Failures);
    }
}