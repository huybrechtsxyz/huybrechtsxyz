using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a measurement unit used for different types such as height, weight, volume, etc.
/// </summary>
/// <remarks>
/// This class stores detailed information about each measurement unit, including its type, code, name, precision, rounding method, and its relationship to the base unit.
/// </remarks>
[MultiTenant]
[Table("SetupUnit")]
[Index(nameof(TenantId), nameof(Code), IsUnique = true)]
[Index(nameof(TenantId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Represents a measurement unit used for different types such as height, weight, volume, etc.")]
public record SetupUnit: Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.).
    /// </summary>
    /// <remarks>
    /// Type defines the category to which the unit belongs, allowing for better organization and lookup.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.).")]
    public SetupUnitType UnitType { get; set; } = SetupUnitType.System;

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
    [MaxLength(128)]
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
    public string? Description { get; set; } = string.Empty;

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

    /// <summary>
    /// Additional notes or remarks related to the unit.
    /// </summary>
    [Comment("Additional remarks or comments about the unit.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}

/// <summary>
/// Enum representing the types of units based on the International System of Units (SI).
/// </summary>
public enum SetupUnitType
{
    /// <summary>
    /// Units of measurement for the system
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(System), Description = nameof(SetupUnitType) + "_" + nameof(System) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for the system")]
    System,

    /// <summary>
    /// Units of measurement for length (e.g., meter, kilometer).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Length), Description = nameof(SetupUnitType) + "_" + nameof(Length) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for length (e.g., meter, kilometer).")]
    Length,

    /// <summary>
    /// Units of measurement for mass (e.g., kilogram, gram).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Mass), Description = nameof(SetupUnitType) + "_" + nameof(Mass) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for mass (e.g., kilogram, gram).")]
    Mass,

    /// <summary>
    /// Units of measurement for time (e.g., second, minute).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Time), Description = nameof(SetupUnitType) + "_" + nameof(Time) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for time (e.g., second, minute).")]
    Time,

    /// <summary>
    /// Units of measurement for electric current (e.g., ampere).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(ElectricCurrent), Description = nameof(SetupUnitType) + "_" + nameof(ElectricCurrent) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for electric current (e.g., ampere).")]
    ElectricCurrent,

    /// <summary>
    /// Units of measurement for temperature (e.g., kelvin, Celsius).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Temperature), Description = nameof(SetupUnitType) + "_" + nameof(Temperature) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for temperature (e.g., kelvin, Celsius).")]
    Temperature,

    /// <summary>
    /// Units of measurement for amount of substance (e.g., mole).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(AmountOfSubstance), Description = nameof(SetupUnitType) + "_" + nameof(AmountOfSubstance) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for amount of substance (e.g., mole).")]
    AmountOfSubstance,

    /// <summary>
    /// Units of measurement for luminous intensity (e.g., candela).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(LuminousIntensity), Description = nameof(SetupUnitType) + "_" + nameof(LuminousIntensity) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for luminous intensity (e.g., candela).")]
    LuminousIntensity,

    /// <summary>
    /// Units of measurement for volume (e.g., liter, cubic meter).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Volume), Description = nameof(SetupUnitType) + "_" + nameof(Volume) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for volume (e.g., liter, cubic meter).")]
    Volume,

    /// <summary>
    /// Units of measurement for area (e.g., square meter).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Area), Description = nameof(SetupUnitType) + "_" + nameof(Area) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for area (e.g., square meter).")]
    Area,

    /// <summary>
    /// Units of measurement for data size (e.g., byte, kilobyte).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(DataSize), Description = nameof(SetupUnitType) + "_" + nameof(DataSize) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for data size (e.g., byte, kilobyte).")]
    DataSize,

    /// <summary>
    /// Units of measurement for speed (e.g., meter per second).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Speed), Description = nameof(SetupUnitType) + "_" + nameof(Speed) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for speed (e.g., meter per second).")]
    Speed,

    /// <summary>
    /// Units of measurement for frequency (e.g., hertz).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Frequency), Description = nameof(SetupUnitType) + "_" + nameof(Frequency) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for frequency (e.g., hertz).")]
    Frequency,

    /// <summary>
    /// Units of measurement for pressure (e.g., pascal).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Pressure), Description = nameof(SetupUnitType) + "_" + nameof(Pressure) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for pressure (e.g., pascal).")]
    Pressure,

    /// <summary>
    /// Units of measurement for energy (e.g., joule).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Energy), Description = nameof(SetupUnitType) + "_" + nameof(Energy) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for energy (e.g., joule).")]
    Energy,

    /// <summary>
    /// Units of measurement for power (e.g., watt).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(Power), Description = nameof(SetupUnitType) + "_" + nameof(Power) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for power (e.g., watt).")]
    Power
}