using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;


namespace Huybrechts.App.Tests.Features.Setup.SetupUnitTests;

public class SetupUnitHelperTests
{
    [Fact]
    public void ConvertUnit_SameUnitType_ReturnsConvertedValue()
    {
        // Arrange
        var fromUnit = new SetupUnit { UnitType = SetupUnitType.Length, Factor = 0.1m, Precision = 2, PrecisionType = MidpointRounding.ToEven };
        var toUnit = new SetupUnit { UnitType = SetupUnitType.Length, Factor = 1.0m, Precision = 2, PrecisionType = MidpointRounding.ToEven };
        decimal valueToConvert = 100m;

        // Act
        decimal convertedValue = SetupUnitHelper.ConvertUnit(valueToConvert, fromUnit, toUnit);

        // Assert
        Assert.Equal(10m, convertedValue);
    }

    [Fact]
    public void ConvertUnit_DifferentUnitTypes_ThrowsArgumentException()
    {
        // Arrange
        var fromUnit = new SetupUnit { UnitType = SetupUnitType.Length };
        var toUnit = new SetupUnit { UnitType = SetupUnitType.Mass };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SetupUnitHelper.ConvertUnit(100m, fromUnit, toUnit));
    }

    [Fact]
    public async Task IsDuplicateNameAsync_ShouldReturnTrue_WhenDuplicateExists()
    {
        // Arrange
        var mockContext = new Mock<DbContext>();
        var setupUnits = new List<SetupUnit>
        {
            new() { Name = "Duplicate", Id = Ulid.NewUlid() }
        };

        mockContext.Setup(x => x.Set<SetupUnit>()).ReturnsDbSet(setupUnits);

        // Act
        var result = await SetupUnitHelper.IsDuplicateNameAsync(mockContext.Object, "Duplicate");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsDuplicateNameAsync_ShouldReturnFalse_WhenNoDuplicateExists()
    {
        // Arrange
        var mockContext = new Mock<DbContext>();
        var setupUnits = new List<SetupUnit>();

        mockContext.Setup(x => x.Set<SetupUnit>()).ReturnsDbSet(setupUnits);

        // Act
        var result = await SetupUnitHelper.IsDuplicateNameAsync(mockContext.Object, "UniqueName");

        // Assert
        Assert.False(result);
    }
}
