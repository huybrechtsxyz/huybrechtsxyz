using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Enum representing the types of units based on the International System of Units (SI).
/// </summary>
public enum SetupUnitType
{
    /// <summary>
    /// Units of measurement for length (e.g., meter, kilometer).
    /// </summary>
    Length,

    /// <summary>
    /// Units of measurement for mass (e.g., kilogram, gram).
    /// </summary>
    Mass,

    /// <summary>
    /// Units of measurement for time (e.g., second, minute).
    /// </summary>
    Time,

    /// <summary>
    /// Units of measurement for electric current (e.g., ampere).
    /// </summary>
    ElectricCurrent,

    /// <summary>
    /// Units of measurement for temperature (e.g., kelvin, Celsius).
    /// </summary>
    Temperature,

    /// <summary>
    /// Units of measurement for amount of substance (e.g., mole).
    /// </summary>
    AmountOfSubstance,

    /// <summary>
    /// Units of measurement for luminous intensity (e.g., candela).
    /// </summary>
    LuminousIntensity,

    /// <summary>
    /// Units of measurement for volume (e.g., liter, cubic meter).
    /// </summary>
    Volume,

    /// <summary>
    /// Units of measurement for area (e.g., square meter).
    /// </summary>
    Area,

    /// <summary>
    /// Units of measurement for data size (e.g., byte, kilobyte).
    /// </summary>
    DataSize,

    /// <summary>
    /// Units of measurement for speed (e.g., meter per second).
    /// </summary>
    Speed,

    /// <summary>
    /// Units of measurement for frequency (e.g., hertz).
    /// </summary>
    Frequency,

    /// <summary>
    /// Units of measurement for pressure (e.g., pascal).
    /// </summary>
    Pressure,

    /// <summary>
    /// Units of measurement for energy (e.g., joule).
    /// </summary>
    Energy,

    /// <summary>
    /// Units of measurement for power (e.g., watt).
    /// </summary>
    Power,

    /// <summary>
    /// Units of measurement for the system
    /// </summary>
    System
}


/// <summary>
/// Represents a measurement unit used for different types such as height, weight, volume, etc.
/// </summary>
/// <remarks>
/// This class stores detailed information about each measurement unit, including its type, code, name, precision, rounding method, and its relationship to the base unit.
/// </remarks>
public record SetupUnit: Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.).
    /// </summary>
    /// <remarks>
    /// Type defines the category to which the unit belongs, allowing for better organization and lookup.
    /// </remarks>
    [Required]
    [MaxLength(32)]
    public SetupUnitType Type { get; set; } = SetupUnitType.System;

    /// <summary>
    /// Gets or sets the code representing the unit (e.g., HOUR, KG).
    /// </summary>
    /// <remarks>
    /// The code is unique within its type and is used to standardize the representation of units.
    /// </remarks>
    [Required]
    [MaxLength(10)]
    [Comment("A unique code representing the unit, standard across all instances.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the unit.
    /// </summary>
    /// <remarks>
    /// Name provides a human-readable description of the unit, which is unique within its type.
    /// </remarks>
    [Required]
    [MaxLength(100)]
    [Comment("The unique name of the unit within its type.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the unit.
    /// </summary>
    /// <remarks>
    /// Provides additional information about the unit, such as its use case or any specific notes.
    /// </remarks>
    [MaxLength(255)]
    [Comment("A detailed description of the unit.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this unit is the base unit for its type.
    /// </summary>
    /// <remarks>
    /// The base unit has a factor of 1.0 and is used as the standard for conversions within the type.
    /// </remarks>
    [Comment("Indicates whether this unit is the base unit for its type.")]
    public bool IsBase { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of decimal places of precision for this unit.
    /// </summary>
    /// <remarks>
    /// Precision determines the number of decimal places allowed for the unit's values.
    /// </remarks>
    [Range(0, 10)]
    [Comment("Number of decimal places for the unit.")]
    public int Precision { get; set; } = 2;

    /// <summary>
    /// Gets or sets the precision type for rounding values of this unit.
    /// </summary>
    /// <remarks>
    /// Uses the System.Decimal Rounding enum to define rounding behavior.
    /// </remarks>
    [Comment("Determines how values are rounded according to the System.Decimal Rounding enum.")]
    public MidpointRounding PrecisionType { get; set; } = MidpointRounding.ToEven;

    /// <summary>
    /// Gets or sets the multiplication factor relative to the base unit.
    /// </summary>
    /// <remarks>
    /// Factor is used to convert values of this unit to the base unit within its type.
    /// </remarks>
    [Precision(18, 10)]
    [Comment("A multiplication factor used to convert this unit to the base unit.")]
    public decimal Factor { get; set; } = 1.0m;
}

