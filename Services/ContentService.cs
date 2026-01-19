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
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        try
        {
            System.IO.File.WriteAllText(jsonPath, json);
            // write a small log alongside editlibrary logs
            try
            {
                var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
                var logPath = Path.Combine(logsDir, "editlibrary.log");
                var entry = $"[{DateTime.UtcNow:O}] ContentService.Save wrote {jsonPath} ({json.Length} bytes)" + Environment.NewLine;
                System.IO.File.AppendAllText(logPath, entry);
            }
            catch { }
        }
        catch
        {
            throw;
        }
    }
}
