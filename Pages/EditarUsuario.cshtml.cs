using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Models;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class EditarUsuarioModel : PageModel
{
    public List<UserRecord> Users { get; set; } = new List<UserRecord>();
    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin") return RedirectToPage("/Login");
        Users = UserService.LoadUsers();
        return Page();
    }

    public IActionResult OnPost()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin") return RedirectToPage("/Login");

        int.TryParse(Request.Form["usersCount"].ToString(), out int count);
        var output = new List<UserRecord>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < count; i++)
        {
            var orig = Request.Form[$"orig_{i}"].ToString() ?? "";
            var del = Request.Form[$"delete_{i}"].ToString();
            if (!string.IsNullOrEmpty(del))
            {
                // delete user's progress file
                try
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "data", "progress");
                    Directory.CreateDirectory(folder);
                    var safeUser = string.Join("_", (orig ?? "").Split(Path.GetInvalidFileNameChars()));
                    var p = Path.Combine(folder, safeUser + ".json");
                    if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
                }
                catch { }
                continue;
            }

            var name = Request.Form[$"name_{i}"].ToString() ?? "";
            var email = Request.Form[$"email_{i}"].ToString() ?? "";
            var password = Request.Form[$"password_{i}"].ToString() ?? "";
            var typeStr = Request.Form[$"type_{i}"].ToString() ?? "1";

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                Error = "Nome e email são obrigatórios.";
                Users = UserService.LoadUsers();
                return Page();
            }

            if (string.Equals(email, "admin@teste.com", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(email, "user@teste.com", StringComparison.OrdinalIgnoreCase))
            {
                Error = "Email reservado não pode ser usado.";
                Users = UserService.LoadUsers();
                return Page();
            }

            if (seen.Contains(email))
            {
                Error = "Emails duplicados não são permitidos.";
                Users = UserService.LoadUsers();
                return Page();
            }
            seen.Add(email);

            if (!int.TryParse(typeStr, out int type)) type = 1;

            output.Add(new UserRecord { Name = name.Trim(), Email = email.Trim(), Password = password, Type = type });
        }

        // save
        try
        {
            UserService.SaveUsers(output);
        }
        catch (Exception ex)
        {
            Error = "Falha ao salvar usuários: " + ex.Message;
            Users = UserService.LoadUsers();
            return Page();
        }

        return RedirectToPage("/EditarUsuario");
    }
}
