using Finbuckle.MultiTenant;
using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Setup;
using Huybrechts.App.Features.Setup.SetupNoSerieFlow;
using Huybrechts.Core.Setup;
using Moq;

namespace Huybrechts.App.Tests.Features.Setup.SetupNoSerieTests;

public class NumberSeriesGeneratorTest
{
    private readonly Mock<IValidator<NoSerieQuery>> _mockValidator;
    private readonly NumberSeriesGenerator _numberSeriesGenerator;
    //private readonly TenantInfo _tenantInfo;

    public NumberSeriesGeneratorTest()
    {
        _mockValidator = new Mock<IValidator<NoSerieQuery>>();
        //_tenantInfo = new TenantInfo() { Id = "1", Name = "Team1", Identifier = "team1" };
        _numberSeriesGenerator = new NumberSeriesGenerator(_mockValidator.Object);
    }

    // Helper method to create a basic SetupNoSerie object
    private static SetupNoSerie CreateSetupNoSerie(string typeOf, string typeValue, int start, int counter, int increment, int maximum, bool isDisabled = false, bool isReset = false)
    {
        return new SetupNoSerie
        {
            TypeOf = typeOf,
            TypeValue = typeValue,
            Format = "PREFIX-{YYYY}-{MM}-{DD}-{####}-SUFFIX",
            StartCounter = start,
            Increment = increment,
            Maximum = maximum,
            AutomaticReset = isReset,
            IsDisabled = isDisabled,
            LastCounter = counter,
            LastValue = "PREFIX-2023-10-09-0005-SUFFIX",
            LastPrefix = "PREFIX-2023-10-11--SUFFIX"
        };
    }

    private static SetupNoSerie CreateNewSetupNoSerie(string typeOf, string typeValue, int start, int increment, int maximum, bool isDisabled = false, bool isReset = false)
    {
        return new SetupNoSerie
        {
            TypeOf = typeOf,
            TypeValue = typeValue,
            Format = "PREFIX-{YYYY}-{MM}-{DD}-{####}-SUFFIX",
            StartCounter = start,
            Increment = increment,
            Maximum = maximum,
            AutomaticReset = isReset,
            IsDisabled = isDisabled,
            LastCounter = 0,
            LastValue = "",
            LastPrefix = ""
        };
    }

    [Fact]
    public void Generate_ShouldReturnValidationFailure_WhenQueryIsInvalid()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = DateTime.Now };
        var validationResult = new ValidationResult(new List<ValidationFailure> { new("TypeOf", "Error") });

        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(validationResult);

        // Act
        var result = _numberSeriesGenerator.Generate(query, []);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Error", result.Errors[0].Message);
    }

    [Fact]
    public void Generate_ShouldFail_WhenSeriesIsNull()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = DateTime.Now };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        // Act
        var result = _numberSeriesGenerator.Generate(query, null!);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(Messages.INVALID_SETUPNOSERIE_CONFIG.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"), result.Errors[0].Message);
    }

    [Fact]
    public void Generate_ShouldFail_WhenSeriesIsEmpty()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = DateTime.Now };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        // Act
        var result = _numberSeriesGenerator.Generate(query, []);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(Messages.INVALID_SETUPNOSERIE_CONFIG.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"), result.Errors[0].Message);
    }

    [Fact]
    public void Generate_ShouldFail_WhenSeriesIsDisabled()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = DateTime.Now };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        var series = new List<SetupNoSerie>
        {
            CreateSetupNoSerie(SetupNoSerieHelper.PROJECTCODE, "001", 1, 5, 1, 10, isDisabled: true)
        };

        // Act
        var result = _numberSeriesGenerator.Generate(query, series);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(Messages.INVALID_SETUPNOSERIE_DISABLED.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"), result.Errors[0].Message);
    }

    [Fact]
    public void Generate_ShouldAutoResetCounter_WhenResetConditionIsMet()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = new DateTime(2024, 01, 01) };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        var series = new List<SetupNoSerie>
        {
            CreateSetupNoSerie(SetupNoSerieHelper.PROJECTCODE, "001", 1, 5, 1, 10, isReset: true) // LastValue is "2023-10-09-Team1-0005"
        };

        // Act
        var result = _numberSeriesGenerator.Generate(query, series);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.LastCounter);  // Counter should reset to minimum and incremented to 6
        Assert.Equal("PREFIX-2024-01-01-000001-SUFFIX", result.Value.LastValue);
    }

    [Fact]
    public void Generate_ShouldIncrementCounter_WhenResetIsNotRequired()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = new DateTime(2023, 10, 10) };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        var series = new List<SetupNoSerie>
        {
            CreateSetupNoSerie(SetupNoSerieHelper.PROJECTCODE, "001", 1, 5, 1, 10)  // Initial counter 5
        };

        // Act
        var result = _numberSeriesGenerator.Generate(query, series);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value.LastCounter);  // Counter should increment by 1
        Assert.Equal("PREFIX-2023-10-10-000006-SUFFIX", result.Value.LastValue);
    }

    [Fact]
    public void Generate_ShouldIncrementCounter_WhenNewCodeIsUsed()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = new DateTime(2023, 10, 10) };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        var series = new List<SetupNoSerie>
        {
            CreateNewSetupNoSerie(SetupNoSerieHelper.PROJECTCODE, "001", 1, 1, 10)  // Initial counter 5
        };

        // Act
        var result = _numberSeriesGenerator.Generate(query, series);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.LastCounter);  // Counter should increment by 1
        Assert.Equal("PREFIX-2023-10-10-000001-SUFFIX", result.Value.LastValue);
    }

    [Fact]
    public void Generate_ShouldFail_WhenCounterExceedsMaximum()
    {
        // Arrange
        var query = new NoSerieQuery { TypeOf = SetupNoSerieHelper.PROJECTCODE, TypeValue = "001", DateTime = new DateTime(2023, 10, 10) };
        _mockValidator.Setup(v => v.Validate(It.IsAny<NoSerieQuery>())).Returns(new ValidationResult());

        var series = new List<SetupNoSerie>
    {
        CreateSetupNoSerie(SetupNoSerieHelper.PROJECTCODE, "001", 1, 10, 1, 10)  // Counter at max (10)
    };

        // Act
        var result = _numberSeriesGenerator.Generate(query, series);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(Messages.INVALID_SETUPNOSERIE_MAXIMUM.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"), result.Errors[0].Message);
    }
}
