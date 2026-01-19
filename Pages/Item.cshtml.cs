using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Models;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class ItemModel : PageModel
{
    public ContentItem? Item { get; private set; }
    public string? TextContent { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public bool IsDone { get; set; }

    public IActionResult OnGet()
    {
        var auth = HttpContext.Session.GetString("auth");
        if (auth != "ok") return RedirectToPage("/Login");

        var user = HttpContext.Session.GetString("user") ?? "anon";

        // ✅ Lê o content.json e cria a variável "content"
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "content.json");
        var json = System.IO.File.ReadAllText(jsonPath);

        var content = JsonSerializer.Deserialize<ContentRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ContentRoot();

        // ✅ Encontra o item pelo Id vindo da querystring (?id=...)
        Item = content.Modules
            .SelectMany(m => m.Lessons)
            .SelectMany(l => l.Items)
            .FirstOrDefault(i => i.Id == Id);

        if (Item is null) return Page();

        // ✅ status concluído
        var doneItems = ProgressService.LoadDoneItems(user);
        IsDone = doneItems.Contains(Item.Id);

        // ✅ Se for texto, ler o arquivo em wwwroot
        if (Item.Type == "text" && Item.Path.StartsWith("/"))
        {
            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                Item.Path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            TextContent = System.IO.File.Exists(physicalPath)
                ? System.IO.File.ReadAllText(physicalPath)
                : "(Arquivo de texto não encontrado em wwwroot)";
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var auth = HttpContext.Session.GetString("auth");
        if (auth != "ok") return RedirectToPage("/Login");

        var user = HttpContext.Session.GetString("user") ?? "anon";

        ProgressService.SetDone(user, Id, IsDone);

        return RedirectToPage(new { id = Id });
    }
}
