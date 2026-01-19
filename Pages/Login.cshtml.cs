using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AulaDeIngles.Pages;

public class LoginModel : PageModel
{
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        // ✅ Login fake (depois troca por banco/identidade)
        if (Email == "admin@teste.com" && Password == "123")
        {
            HttpContext.Session.SetString("auth", "ok");
            HttpContext.Session.SetString("user", Email); // ✅ novo
            return RedirectToPage("/Library");
        }

        Error = "Credenciais inválidas.";
        return Page();
    }
}
