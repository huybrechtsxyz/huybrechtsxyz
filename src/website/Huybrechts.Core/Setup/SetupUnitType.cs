using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Enum representing the types of units based on the International System of Units (SI).
/// </summary>
/// <remarks>
/// This enum is used for defining the different categories of measurement units, such as length, mass, time, etc.
/// It is based on the SI units but can also include other commonly used unit types.
/// </remarks>
public enum SetupUnitType
{
    /// <summary>
    /// Units of measurement for the system (e.g., system-specific units not covered by other categories).
    /// </summary>
    [Display(Name = nameof(SetupUnitType) + "_" + nameof(System), Description = nameof(SetupUnitType) + "_" + nameof(System) + "_d", ResourceType = typeof(Localization))]
    [Comment("Units of measurement for the system")]
    System = 1,

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