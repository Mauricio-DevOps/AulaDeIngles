namespace AulaDeIngles.Models;

public class UserRecord
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = ""; // simple storage for now
    public int Type { get; set; } = 1; // 1..4
}
