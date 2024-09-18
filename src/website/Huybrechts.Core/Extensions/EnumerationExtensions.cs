using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Huybrechts.Core.Extensions;

public static class EnumerationExtensions
{
    /// <summary>
    /// Get the Name from the DisplayAttribute.
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetName(this Enum enumValue)
    {
        return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?
                   .GetName() ?? string.Empty;
    }

    /// <summary>
    /// Get the Description from the DisplayAttribute.
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?
                   .GetDescription() ?? string.Empty;
    }

    /// <summary>
    /// Get the Prompt from the DisplayAttribute.
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetPrompt(this Enum enumValue)
    {
        return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?
                   .GetPrompt() ?? string.Empty;
    }
}
