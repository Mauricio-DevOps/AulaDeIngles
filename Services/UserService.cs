using System.Text.Json;
using AulaDeIngles.Models;

namespace AulaDeIngles.Services;

public static class UserService
{
    private static string UsersPath()
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "data");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "users.json");
    }

    public static List<UserRecord> LoadUsers()
    {
        var path = UsersPath();
        if (!File.Exists(path)) return new List<UserRecord>();
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<UserRecord>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserRecord>();
        }
        catch
        {
            return new List<UserRecord>();
        }
    }

    public static void SaveUsers(List<UserRecord> users)
    {
        var path = UsersPath();
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static UserRecord? FindByEmail(string email)
    {
        return LoadUsers().FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public static bool Authenticate(string email, string password, out UserRecord? user)
    {
        user = FindByEmail(email);
        if (user == null) return false;
        return user.Password == password;
    }
}
