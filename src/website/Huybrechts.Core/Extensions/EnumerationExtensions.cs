using System.ComponentModel;
using System.Reflection;

namespace Huybrechts.Core.Extensions;

public static class EnumerationExtensions
{
    /// <summary>
    /// Get the Description from the DescriptionAttribute.
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum enumValue)
    {
        return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DescriptionAttribute>()?
                   .Description ?? string.Empty;
    }
}
