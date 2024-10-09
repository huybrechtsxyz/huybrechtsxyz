using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a general-purpose numbering series used for generating sequential numbers for various entities (e.g., Projects, Invoices) 
/// in a multi-tenant system.
/// </summary>
/// <remarks>
/// This class is responsible for maintaining the current counter and format of number series for different types of entities
/// (e.g., project types or invoice numbers). Each type and value combination can have its own series with a specific format.
/// </remarks>
[MultiTenant]
[Table("SetupNoSerie")]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Stores configuration for number series, supporting multi-tenancy.")]
public record SetupNoSerie : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of number series (e.g., ProjectNumber, InvoiceNumber).
    /// </summary>
    /// <remarks>
    /// This field identifies the type of entity that the number series applies to, such as projects or invoices.
    /// </remarks>
    [MaxLength(64)]
    [Comment("Type of number series, such as ProjectNumber or InvoiceNumber.")]
    public string TypeOf { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type code (e.g., ProjectType or InvoiceType).
    /// </summary>
    /// <remarks>
    /// This field defines the base category for the number series. For example, it can represent the project type or
    /// invoice type that the number series applies to.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The category on which the number series is based, such as ProjectType or InvoiceType.")]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the specific value within the type code (e.g., Sales, Development).
    /// </summary>
    /// <remarks>
    /// This field stores the specific value for the corresponding type code. For example, in the case of a project number series, 
    /// this could represent different project categories like Sales or Development, each with its own numbering sequence.
    /// </remarks>
    [MaxLength(64)]
    [Comment("Specific value within the type code, such as 'Sales' or 'Development' for project types.")]
    public string TypeValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format string for the number series.
    /// </summary>
    /// <remarks>
    /// The format defines how the number will be generated. This can include placeholders for dates (e.g., YYYY-MM) and sequential numbers 
    /// (e.g., ### or (000)).
    /// Example format: "SP-YYYY-MM-###" where the number will include the year, month, and a sequential number.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Format string defining how the number is generated, including placeholders like YYYY-MM-###.")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current counter for this series.
    /// </summary>
    /// <remarks>
    /// This field stores the current sequential number for the series. Each time a new number is generated, this value is incremented.
    /// </remarks>
    [Comment("The current sequential number for this series.")]
    public int Counter { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum value for the counter.
    /// </summary>
    /// <remarks>
    /// The counter will reset or stop incrementing when it reaches this maximum value.
    /// </remarks>
    [Comment("The maximum allowed value for the counter before it resets or stops.")]
    public int Maximum { get; set; } = 999999999;

    /// <summary>
    /// Gets or sets a value indicating whether the counter is disabled.
    /// </summary>
    /// <remarks>
    /// If this flag is set to true, the counter is disabled, and no new numbers will be generated for this series.
    /// </remarks>
    [Comment("Indicates whether the counter is disabled. If true, the counter is inactive.")]
    public bool IsDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets a description of the number series.
    /// </summary>
    /// <remarks>
    /// This field can be used to provide additional context or information about the purpose of this specific number series.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Optional description providing more details about the number series.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the search index, which stores the normalized concatenated values for efficient searching.
    /// </summary>
    /// <remarks>
    /// This field is used to optimize search operations by storing a normalized version of key fields (e.g., TypeOf, TypeCode, TypeValue). 
    /// It allows for faster retrieval of number series in search queries.
    /// </remarks>
    [Comment("A normalized concatenated field used for optimizing search operations.")]
    public string? SearchIndex { get; set; }
}
