using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

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
            HttpContext.Session.SetString("user", Email);
            HttpContext.Session.SetString("role", "admin");
            return RedirectToPage("/Library");
        }

        if (Email == "user@teste.com" && Password == "123")
        {
            HttpContext.Session.SetString("auth", "ok");
            HttpContext.Session.SetString("user", Email);
            HttpContext.Session.SetString("role", "user");
            return RedirectToPage("/Library");
        }

        // check persisted users
        try
        {
            if (AulaDeIngles.Services.UserService.Authenticate(Email, Password, out var found))
            {
                HttpContext.Session.SetString("auth", "ok");
                HttpContext.Session.SetString("user", Email);
                HttpContext.Session.SetString("role", "student");
                if (found != null)
                {
                    HttpContext.Session.SetString("userType", found.Type.ToString());
                }
                return RedirectToPage("/Library");
            }
        }
        catch { /* ignore */ }

        Error = "Credenciais inválidas.";
        return Page();
    }
}
