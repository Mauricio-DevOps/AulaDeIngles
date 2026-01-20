using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Models;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class CadastrarAlunoModel : PageModel
{
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public int Type { get; set; } = 1;

    public string? Error { get; set; }
    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin") return RedirectToPage("/Login");
        return Page();
    }

    public IActionResult OnPost()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin") return RedirectToPage("/Login");

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Preencha nome, email e senha.";
            return Page();
        }

        if (Type < 1 || Type > 4)
        {
            Error = "Tipo inválido.";
            return Page();
        }

        // disallow admin/user duplicates
        if (string.Equals(Email, "admin@teste.com", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Email, "user@teste.com", StringComparison.OrdinalIgnoreCase))
        {
            Error = "Este email não pode ser usado.";
            return Page();
        }

        var existing = UserService.FindByEmail(Email);
        if (existing != null)
        {
            Error = "Já existe um usuário com esse email.";
            return Page();
        }

        var user = new UserRecord { Name = Name.Trim(), Email = Email.Trim(), Password = Password, Type = Type };
        var users = UserService.LoadUsers();
        users.Add(user);
        UserService.SaveUsers(users);

        // create empty progress file so user starts fresh
        try
        {
            AulaDeIngles.Services.ProgressService.SetDone(user.Email, "__init__", false);
        }
        catch { }

        return RedirectToPage("/Library");
    }
}
