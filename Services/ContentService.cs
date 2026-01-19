using System.Text.Json;
using AulaDeIngles.Models;

namespace AulaDeIngles.Services;

public static class ContentService
{
    public static ContentRoot Load()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "content.json");
        var json = System.IO.File.ReadAllText(jsonPath);

        return JsonSerializer.Deserialize<ContentRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ContentRoot();
    }

    public static void Save(ContentRoot content)
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "content.json");
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(jsonPath, json);
    }
}
