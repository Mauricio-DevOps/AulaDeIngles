using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.IO;
using AulaDeIngles.Models;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class EditLibraryModel : PageModel
{
    [BindProperty]
    public string JsonContent { get; set; } = "";

    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin")
            return RedirectToPage("/Login");

        var content = ContentService.Load();
        JsonContent = JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });
        return Page();
    }

    public IActionResult OnPost()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin")
            return RedirectToPage("/Login");

        try
        {
            var content = JsonSerializer.Deserialize<ContentRoot>(JsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (content == null)
            {
                Error = "Conteúdo inválido.";
                return Page();
            }

            // Basic validation: ensure modules list exists
            content.Modules ??= new List<Module>();

            ContentService.Save(content);

            return RedirectToPage("/Library");
        }
        catch (Exception ex)
        {
            Error = "Erro ao salvar: " + ex.Message;
            return Page();
        }
    }
}
