using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Setup;

namespace Huybrechts.App.Tests;

public class SetupUnitConversionTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void FromCentimeterToMeter()
    {
        var meterModel = new SetupUnit
        {
            Id = Ulid.NewUlid(),
            Code = "m",
            Name = "Meter",
            UnitType = SetupUnitType.Length,
            Factor = 1.0m, // Base unit
            Precision = 2,
            PrecisionType = MidpointRounding.ToEven
        };

        var centimeterModel = new SetupUnit
        {
            Id = Ulid.NewUlid(),
            Code = "cm",
            Name = "Centimeter",
            UnitType = SetupUnitType.Length,
            Factor = 0.01m, // Centimeters to meters
            Precision = 2,
            PrecisionType = MidpointRounding.ToEven
        };

        // Convert 150 centimeters to meters
        decimal result = SetupUnitHelper.ConvertUnit(150, centimeterModel, meterModel);
        Assert.That(result, Is.EqualTo(1.5));
    }
}