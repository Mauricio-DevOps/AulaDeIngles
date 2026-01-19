using System.Text.Json;

namespace AulaDeIngles.Services;

public class ProgressService
{
    private static string GetFilePath(string user)
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "data", "progress");
        Directory.CreateDirectory(folder);
        // evita caracteres ruins no nome do arquivo
        var safeUser = string.Join("_", user.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(folder, $"{safeUser}.json");
    }

    public static HashSet<string> LoadDoneItems(string user)
    {
        var path = GetFilePath(user);
        if (!File.Exists(path)) return new HashSet<string>();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
    }

    public static void SetDone(string user, string itemId, bool done)
    {
        var doneItems = LoadDoneItems(user);

        if (done) doneItems.Add(itemId);
        else doneItems.Remove(itemId);

        var path = GetFilePath(user);
        var json = JsonSerializer.Serialize(doneItems, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
