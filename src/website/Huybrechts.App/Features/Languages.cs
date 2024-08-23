using Huybrechts.Core.Setup;
using System.Text.Json;

namespace Huybrechts.App.Features;

public static class Languages
{
    private static bool loaded = false;
    private static List<LanguageInfo> items = [];

    public static List<LanguageInfo> Items 
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
        var filePath = Path.Combine("./languages.json");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The languages.json file was not found.", filePath);
        }

        try
        {
            // Read the JSON file asynchronously
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var languages = await JsonSerializer.DeserializeAsync<List<LanguageInfo>>(stream);
            items = languages ?? [];
            loaded = true;
        }
        catch (Exception ex)
        {
            // Handle potential errors
            throw new ApplicationException($"Failed to read languages from {filePath}: {ex.Message}", ex);
        }
    }
}
