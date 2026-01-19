using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class ModuleModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public AulaDeIngles.Models.Module? ModuleData { get; private set; }
    public HashSet<string> DoneItems { get; private set; } = new();

    public int ModuleTotal { get; private set; }
    public int ModuleDone { get; private set; }

    public IActionResult OnGet()
    {
        var auth = HttpContext.Session.GetString("auth");
        if (auth != "ok") return RedirectToPage("/Login");

        var user = HttpContext.Session.GetString("user") ?? "anon";

        var content = ContentService.Load();
        ModuleData = content.Modules.FirstOrDefault(m => m.Id == Id);

        DoneItems = ProgressService.LoadDoneItems(user);

        if (ModuleData != null)
        {
            ModuleTotal = ModuleData.Lessons.Sum(l => l.Items.Count);
            ModuleDone = ModuleData.Lessons.Sum(l => l.Items.Count(i => DoneItems.Contains(i.Id)));
        }

        return Page();
    }
}
