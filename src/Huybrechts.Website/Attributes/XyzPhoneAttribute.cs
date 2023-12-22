﻿using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Huybrechts.Website.Attributes;

/// <summary>
/// Phone Data Attribute Type
/// </summary>
/// <see cref="https://referencesource.microsoft.com/System.ComponentModel.DataAnnotations/DataAnnotations/PhoneAttribute.cs.html"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class XyzPhoneAttribute : DataTypeAttribute
{
    // see unit tests for examples
    private static Regex _regex = CreateRegEx();
    private const string _additionalPhoneNumberCharacters = "-.()";

    public XyzPhoneAttribute()
        : base(DataType.PhoneNumber)
    {

        // DevDiv 468241: set DefaultErrorMessage not ErrorMessage, allowing user to set
        // ErrorMessageResourceType and ErrorMessageResourceName to use localized messages.
        //ErrorMessage = DataAnnotationsResources.PhoneAttribute_Invalid;
        ErrorMessage = "The Phone number field is not a valid phone number.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        string valueAsString = value as string ?? string.Empty;
        if (string.IsNullOrEmpty(valueAsString))
            return true;

        // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
        if (_regex != null)
        {
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }
        else
        {
            if (valueAsString == null)
            {
                return false;
            }

            valueAsString = valueAsString.Replace("+", string.Empty).TrimEnd();
            valueAsString = RemoveExtension(valueAsString);

            bool digitFound = false;
            foreach (char c in valueAsString)
            {
                if (Char.IsDigit(c))
                {
                    digitFound = true;
                    break;
                }
            }

            if (!digitFound)
            {
                return false;
            }

            foreach (char c in valueAsString)
            {
                if (!(Char.IsDigit(c)
                    || Char.IsWhiteSpace(c)
                    || _additionalPhoneNumberCharacters.IndexOf(c) != -1))
                {
                    return false;
                }
            }
            return true;
        }
    }

    private static Regex CreateRegEx()
    {
        const string pattern = @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$";
        const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

        // Set explicit regex match timeout, sufficient enough for phone parsing
        // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
        TimeSpan matchTimeout = TimeSpan.FromSeconds(2);

        try
        {
            if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null)
            {
                return new Regex(pattern, options, matchTimeout);
            }
        }
        catch
        {
            // Fallback on error
        }

        // Legacy fallback (without explicit match timeout)
        return new Regex(pattern, options);
    }

    private static string RemoveExtension(string potentialPhoneNumber)
    {
        int lastIndexOfExtension = potentialPhoneNumber
            .LastIndexOf("ext.", StringComparison.InvariantCultureIgnoreCase);
        if (lastIndexOfExtension >= 0)
        {
            string extension = potentialPhoneNumber.Substring(lastIndexOfExtension + 4);
            if (MatchesExtension(extension))
            {
                return potentialPhoneNumber.Substring(0, lastIndexOfExtension);
            }
        }

        lastIndexOfExtension = potentialPhoneNumber
            .LastIndexOf("ext", StringComparison.InvariantCultureIgnoreCase);
        if (lastIndexOfExtension >= 0)
        {
            string extension = potentialPhoneNumber.Substring(lastIndexOfExtension + 3);
            if (MatchesExtension(extension))
            {
                return potentialPhoneNumber.Substring(0, lastIndexOfExtension);
            }
        }


        lastIndexOfExtension = potentialPhoneNumber
            .LastIndexOf("x", StringComparison.InvariantCultureIgnoreCase);
        if (lastIndexOfExtension >= 0)
        {
            string extension = potentialPhoneNumber.Substring(lastIndexOfExtension + 1);
            if (MatchesExtension(extension))
            {
                return potentialPhoneNumber.Substring(0, lastIndexOfExtension);
            }
        }

        return potentialPhoneNumber;
    }

    private static bool MatchesExtension(string potentialExtension)
    {
        potentialExtension = potentialExtension.TrimStart();
        if (potentialExtension.Length == 0)
        {
            return false;
        }

        foreach (char c in potentialExtension)
        {
            if (!Char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}