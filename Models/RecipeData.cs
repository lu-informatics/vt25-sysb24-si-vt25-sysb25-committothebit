using System.Text.Json;
using System.Collections.Generic;

namespace Informatics.Appetite.Models;

public class RecipeData
{
    public List<string> steps { get; set; } = new();

    private static JsonSerializerOptions DefaultOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true // Makes the JSON more readable
    };

    public static RecipeData? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        
        try
        {
            return JsonSerializer.Deserialize<RecipeData>(json, DefaultOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON Parse Error: {ex.Message}");
            return null;
        }
    }

    public string ToJson()
    {
        try
        {
            return JsonSerializer.Serialize(this, DefaultOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"JSON Serialize Error: {ex.Message}");
            return "{}"; // Return empty object if serialization fails
        }
    }

    // Helper method to create a new RecipeData with steps
    public static RecipeData Create(IEnumerable<string> steps)
    {
        return new RecipeData { steps = steps.ToList() };
    }

    // Optional convenience method
    public static string StepsToJson(IEnumerable<string> steps)
    {
        return Create(steps).ToJson();
    }
} 