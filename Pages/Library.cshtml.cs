using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Models;
using AulaDeIngles.Services;


namespace AulaDeIngles.Pages;

public class LibraryModel : PageModel
{
    public ContentRoot Content { get; private set; } = new();
    public HashSet<string> DoneItems { get; private set; } = new();


    public IActionResult OnGet()
    {
        var auth = HttpContext.Session.GetString("auth");
        var user = HttpContext.Session.GetString("user") ?? "anon";
        DoneItems = ProgressService.LoadDoneItems(user);

        if (auth != "ok") return RedirectToPage("/Login");

        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "content.json");
        var json = System.IO.File.ReadAllText(jsonPath);

        Content = JsonSerializer.Deserialize<ContentRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ContentRoot();

        return Page();
    }
}
