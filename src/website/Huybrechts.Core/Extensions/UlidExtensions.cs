namespace Huybrechts.Core.Extensions;

public static class UlidExtensions
{
    public static bool IsEmpty(this Ulid? id)
    {
        return !id.HasValue || id.Equals(Ulid.Empty);
    }
}
