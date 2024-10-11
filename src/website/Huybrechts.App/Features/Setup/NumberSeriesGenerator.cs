using Finbuckle.MultiTenant.Abstractions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Setup.SetupNoSerieFlow;
using Huybrechts.App.Web;
using Huybrechts.Core.Setup;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Huybrechts.App.Features.Setup;

/// <summary>
/// Generates and manages a series of formatted numbers based on given configurations and queries.
/// </summary>
public partial class NumberSeriesGenerator
{
    /// <summary>
    /// Validator for validating input queries related to number series generation.
    /// </summary>
    private readonly IValidator<NoSerieQuery>? _validator;

    /// <summary>
    /// Regex pattern used for matching number series counter placeholders in format strings.
    /// </summary>
    private readonly Regex _numberSeriesRegex = NumberSeriesCounterRegex();

    /// <summary>
    /// Compiled regex pattern to match the number series counter placeholder.
    /// </summary>
    [GeneratedRegex(@"\{\#+\}", RegexOptions.Compiled)]
    private static partial Regex NumberSeriesCounterRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberSeriesGenerator"/> class.
    /// </summary>
    /// <param name="tenantInfo">The tenant information used in generating number series.</param>
    /// <param name="validator">The validator for validating number series queries.</param>
    public NumberSeriesGenerator(IValidator<NoSerieQuery>? validator = null)
    {
        _validator = validator;
    }

    /// <summary>
    /// Generates the next number in the series based on the provided query and existing series.
    /// </summary>
    /// <param name="query">The query containing information for number series generation.</param>
    /// <param name="series">A list of existing number series configurations.</param>
    /// <returns>A result containing the generated number series or error information.</returns>
    public Result<SetupNoSerie> Generate(NoSerieQuery query, List<SetupNoSerie> series)
    {
        // Validate input query
        if (_validator is not null)
        {
            ValidationResult validation = _validator.Validate(query);
            if (!validation.IsValid) // Check if validation failed
                return validation.AsResult(); // Return validation errors
        }

        // Validate series data
        if (series is null || series.Count == 0) // No series provided
            return Result.Fail(Messages.INVALID_SETUPNOSERIE_CONFIG.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"));

        // Find the specific series entity or a default one
        SetupNoSerie? entity = series.FirstOrDefault(q => q.TypeOf == query.TypeOf && q.TypeValue == query.TypeValue);
        entity ??= series.FirstOrDefault(q => q.TypeOf == query.TypeOf && q.TypeValue == string.Empty); // Default fallback
        if (entity is null || entity.IsDisabled) // Validate entity's existence and state
            return Result.Fail(entity == null
                ? Messages.INVALID_SETUPNOSERIE_CONFIG.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}")
                : Messages.INVALID_SETUPNOSERIE_DISABLED.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}")
                );

        // Handle automatic counter reset based on conditions
        string newPrefix = GetNumberPrefixWithoutCounter(entity.Format, query.DateTime);
        if (entity.AutomaticReset && !newPrefix.Equals(entity.LastPrefix)) // Reset logic
            entity.LastCounter = entity.StartCounter; // Reset to start counter
        
        // Else increment the counter and validate maximum limit
        else entity.LastCounter += entity.Increment;
        if (entity.LastCounter > entity.Maximum) // Check if counter exceeds maximum
            return Result.Fail(Messages.INVALID_SETUPNOSERIE_MAXIMUM.Replace("{0}", $"{query.TypeOf} - {query.TypeValue}"));

        // Generate the next value after handling the reset logic
        entity.LastValue = GetNextNumber(entity.Format, entity.LastCounter, query.DateTime); // Generate next number
        entity.LastPrefix = newPrefix; // Store last prefix used

        return Result.Ok(entity); // Return successful result with entity
    }

    /// <summary>
    /// Generates the next number based on the specified format and counter.
    /// </summary>
    /// <param name="format">The format string containing placeholders for the number series.</param>
    /// <param name="counter">The current counter value to be used in the series.</param>
    /// <param name="dateTime">The date and time context for generating the number prefix.</param>
    /// <returns>The formatted next number in the series.</returns>
    private string GetNextNumber(string format, int counter, DateTime dateTime)
    {
        if (string.IsNullOrEmpty(format)) return string.Empty; // Handle empty format

        format = GetNumberPrefix(format, dateTime); // Get formatted prefix

        var match = _numberSeriesRegex.Match(format); // Match against regex
        if (match.Success) // If regex matched
        {
            int hashCount = match.Length; // Count number of `#` symbols
            string paddedNumber = counter.ToString($"D{hashCount}"); // Pad the counter
            format = format[..match.Index] + paddedNumber + format[(match.Index + hashCount)..]; // Construct next number
        }

        return format; // Return the formatted next number
    }

    /// <summary>
    /// Extracts the prefix from the specified format without including the counter.
    /// </summary>
    /// <param name="format">The format string from which to extract the prefix.</param>
    /// <param name="dateTime">The date and time context for generating the prefix.</param>
    /// <returns>The prefix without the counter portion.</returns>
    private string GetNumberPrefixWithoutCounter(string format, DateTime dateTime)
    {
        string value = GetNumberPrefix(format, dateTime); // Get formatted prefix
        value = value
            .Replace("#", string.Empty) // Remove `#` symbols
            .Replace("}", string.Empty) // Remove braces
            .Replace("{", string.Empty);
        return value; // Return cleaned prefix
    }

    /// <summary>
    /// Generates a formatted number prefix based on the specified format and current date.
    /// </summary>
    /// <param name="format">The format string used for generating the prefix.</param>
    /// <param name="dateTime">The date and time context for generating the prefix.</param>
    /// <returns>The formatted prefix containing date and tenant-specific information.</returns>
    private string GetNumberPrefix(string format, DateTime dateTime)
    {
        string value = format.ToUpperInvariant() // Format to upper case
            .Replace("{YYYY}", dateTime.ToString("yyyy")) // Full year
            .Replace("{YY}", dateTime.ToString("yy")) // Two-digit year
            .Replace("{MM}", dateTime.ToString("MM")) // Month
            .Replace("{WW}", GetWeekNumber(dateTime)) // Week number
            .Replace("{DD}", dateTime.ToString("dd")); // Day
        return value; // Return formatted value
    }

    /// <summary>
    /// Gets the week number of the year for the specified date.
    /// </summary>
    /// <param name="dateTime">The date for which to get the week number.</param>
    /// <returns>The week number formatted as a two-digit string.</returns>
    private string GetWeekNumber(DateTime dateTime)
    {
        CultureInfo culture = CultureInfo.CurrentCulture; // Get current culture
        Calendar calendar = culture.Calendar; // Get the calendar from the culture
        return calendar.GetWeekOfYear(dateTime, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek).ToString("D2"); // Return week number formatted as two digits
    }
}
