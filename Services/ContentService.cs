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
}
