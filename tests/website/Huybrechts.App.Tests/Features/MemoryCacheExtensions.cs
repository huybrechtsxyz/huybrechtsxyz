using Microsoft.Extensions.Caching.Memory;

namespace Huybrechts.App.Tests.Features;

public static class MemoryCacheExtensions
{
    public static bool TryGetValue<T>(this IMemoryCache cache, object key, out T? value) where T : class
    {
        // Use the underlying IMemoryCache to try to get the value
        if (cache.TryGetValue(key, out var cachedValue))
        {
            value = cachedValue as T; // Cast the cached value to the specified type
            return value != null; // Return true if the value was found and cast successfully
        }

        value = default; // Set value to default if not found
        return false; // Indicate that the value was not found
    }
}