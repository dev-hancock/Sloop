namespace Sloop.Tests.Unit;

using Microsoft.Extensions.Options;

public class SloopOptionsValidatorTests
{
    private readonly SloopOptionsValidator _validator = new();

    [Fact]
    public void Configure_ShouldThrow_WhenConnectionStringIsNull()
    {
        var options = new SloopOptions
        {
            ConnectionString = null!,
            SchemaName = "public",
            TableName = "cache",
            DefaultExpiration = TimeSpan.FromMinutes(5)
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("ConnectionString must be provided.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenSchemaNameIsNull()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = null!,
            TableName = "cache",
            DefaultExpiration = TimeSpan.FromMinutes(5)
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("SchemaName must be provided.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenTableNameIsNull()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = "public",
            TableName = null!,
            DefaultExpiration = TimeSpan.FromMinutes(5)
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("TableName must be provided.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenDefaultExpirationIsNonPositive()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = "public",
            TableName = "cache",
            DefaultExpiration = TimeSpan.Zero
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("DefaultExpiration must be a positive TimeSpan.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenCleanupIntervalIsNonPositive()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = "public",
            TableName = "cache",
            DefaultExpiration = TimeSpan.FromMinutes(5),
            CleanupInterval = TimeSpan.Zero
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("CleanupInterval must be a positive TimeSpan if specified.", ex.Failures);
    }

    [Fact]
    public void Configure_ShouldSucceed_WhenOptionsAreValid()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = "public",
            TableName = "cache",
            DefaultExpiration = TimeSpan.FromMinutes(5),
            CleanupInterval = TimeSpan.FromMinutes(10)
        };

        var exception = Record.Exception(() => _validator.Configure(options));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_ShouldThrow_WhenConnectionFactoryIsNull()
    {
        var options = new SloopOptions
        {
            ConnectionString = "Host=localhost;",
            SchemaName = "public",
            TableName = "cache",
            DefaultExpiration = TimeSpan.FromMinutes(5),
            CleanupInterval = TimeSpan.FromMinutes(5),
            ConnectionFactory = null!
        };

        var ex = Assert.Throws<OptionsValidationException>(() => _validator.Configure(options));

        Assert.Contains("ConnectionFactory must not be null.", ex.Failures);
    }
}