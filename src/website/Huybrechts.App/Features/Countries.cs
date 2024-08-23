using Huybrechts.Core.Setup;
using System.Text.Json;

namespace Huybrechts.App.Features;

public static class Countries
{
    private static bool loaded = false;
    private static List<CountryInfo> items = [];

    public static List<CountryInfo> Items 
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
        var filePath = Path.Combine("./countries.json");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The countries.json file was not found.", filePath);
        }

        try
        {
            // Read the JSON file asynchronously
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var countries = await JsonSerializer.DeserializeAsync<List<CountryInfo>>(stream);
            items = countries ?? [];
            loaded = true;
        }
        catch (Exception ex)
        {
            // Handle potential errors
            throw new ApplicationException($"Failed to read countries from {filePath}: {ex.Message}", ex);
        }
    }
}
