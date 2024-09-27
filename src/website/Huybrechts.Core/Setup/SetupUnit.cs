using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a measurement unit used for different types such as length, mass, volume, etc.
/// </summary>
/// <remarks>
/// This class stores detailed information about each measurement unit, including its type, code, name, precision, rounding method, 
/// and its relationship to the base unit for conversion.
/// Units are categorized by the <see cref="SetupUnitType"/> enum, and each unit can be converted to its base unit using the <see cref="Factor"/> property.
/// </remarks>
[MultiTenant]
[Table("SetupUnit")]
[Index(nameof(TenantId), nameof(Code), IsUnique = true)]
[Index(nameof(TenantId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Represents a measurement unit used for different types such as length, mass, volume, etc.")]
public record SetupUnit : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of the unit (e.g., Length, Mass, Volume, System, etc.).
    /// </summary>
    /// <remarks>
    /// The type categorizes the unit, allowing it to be part of a system that organizes similar measurement units.
    /// For example, 'Length' may include units like meters or kilometers.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the type of the unit (e.g., Length, Mass, Volume, System, etc.).")]
    public SetupUnitType UnitType { get; set; } = SetupUnitType.System;

    /// <summary>
    /// Gets or sets the code representing the unit (e.g., HOUR, KG).
    /// </summary>
    /// <remarks>
    /// The code serves as a short identifier for the unit, commonly used for internal references and to ensure standardization across different systems.
    /// Codes must be unique within their type to avoid confusion during calculations or conversions.
    /// </remarks>
    [Required]
    [MaxLength(10)]
    [Comment("A unique code representing the unit, standardized across all instances.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the unit.
    /// </summary>
    /// <remarks>
    /// The name provides a human-readable representation of the unit, such as "Kilogram" or "Hour", and is unique within its type.
    /// This helps users understand the unit's purpose more easily.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    [Comment("The unique name of the unit within its type.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the unit.
    /// </summary>
    /// <remarks>
    /// Provides additional information about the unit, such as its use case or notes regarding its precision or conversion details.
    /// This can be helpful for documentation or user interfaces where extra context is needed.
    /// </remarks>
    [MaxLength(255)]
    [Comment("A detailed description of the unit.")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this unit is the base unit for its type.
    /// </summary>
    /// <remarks>
    /// The base unit is the default measurement within its category. For example, the base unit for length might be the meter.
    /// All other units in the category can be converted to this base unit using the <see cref="Factor"/>.
    /// </remarks>
    [Comment("Indicates whether this unit is the base unit for its type.")]
    public bool IsBase { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of decimal places of precision for this unit.
    /// </summary>
    /// <remarks>
    /// The precision determines how many decimal places are allowed for values of this unit.
    /// For example, precision of 2 allows values like 12.34. Precision is critical for applications requiring significant accuracy.
    /// </remarks>
    [Range(0, 10)]
    [Comment("Number of decimal places for the unit.")]
    public int Precision { get; set; } = 2;

    /// <summary>
    /// Gets or sets the rounding type for values of this unit.
    /// </summary>
    /// <remarks>
    /// This defines how values are rounded when precision exceeds the allowed number of decimal places.
    /// Rounding types, like <see cref="MidpointRounding.ToEven"/>, help ensure accuracy in different contexts (e.g., financial or scientific).
    /// </remarks>
    [Comment("Determines how values are rounded according to the System.Decimal Rounding enum.")]
    public MidpointRounding PrecisionType { get; set; } = MidpointRounding.ToEven;

    /// <summary>
    /// Gets or sets the multiplication factor relative to the base unit.
    /// </summary>
    /// <remarks>
    /// The factor is used to convert a unit to its base unit. For example, if the base unit is meters and this unit is centimeters,
    /// the factor might be 0.01, meaning 1 cm equals 0.01 meters.
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
    /// This field stores normalized, concatenated values for search purposes.
    /// </summary>
    /// <remarks>
    /// The SearchIndex is pre-computed to optimize searches by concatenating values like Code, Name, and other relevant properties.
    /// </remarks>
    [Comment("This field will store the normalized, concatenated values for searching.")]
    public string? SearchIndex { get; set; }
}
