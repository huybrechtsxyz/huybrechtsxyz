using Huybrechts.Core.Setup;
using System.Text.Json;

namespace Huybrechts.App.Features;

public static class Currencies
{
    private static bool loaded = false;
    private static List<CurrencyInfo> items = [];

    public static List<CurrencyInfo> Items 
    { 
        get
        {  
            if (!loaded)
                LoadAsync().Wait();
            return items;
        }
    }

    public static async Task LoadAsync()
    {
        var filePath = Path.Combine("./currencies.json");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The currencies.json file was not found.", filePath);
        }

        try
        {
            // Read the JSON file asynchronously
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var currencies = await JsonSerializer.DeserializeAsync<List<CurrencyInfo>>(stream);
            items = currencies ?? [];
            loaded = true;
        }
        catch (Exception ex)
        {
            // Handle potential errors
            throw new ApplicationException($"Failed to read currencies from {filePath}: {ex.Message}", ex);
        }
    }
}
