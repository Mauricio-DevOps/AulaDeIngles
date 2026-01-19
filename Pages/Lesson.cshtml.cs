using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AulaDeIngles.Services;

namespace AulaDeIngles.Pages;

public class LessonModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public AulaDeIngles.Models.Module? ModuleData { get; private set; }
    public AulaDeIngles.Models.Lesson? LessonData { get; private set; }

    public HashSet<string> DoneItems { get; private set; } = new();

    public int LessonTotal { get; private set; }
    public int LessonDone { get; private set; }

    public IActionResult OnGet()
    {
        var auth = HttpContext.Session.GetString("auth");
        if (auth != "ok") return RedirectToPage("/Login");

        var user = HttpContext.Session.GetString("user") ?? "anon";

        var content = ContentService.Load();

        ModuleData = content.Modules.FirstOrDefault(m => m.Lessons.Any(l => l.Id == Id));
        LessonData = ModuleData?.Lessons.FirstOrDefault(l => l.Id == Id);

        DoneItems = ProgressService.LoadDoneItems(user);

        if (LessonData != null)
        {
            LessonTotal = LessonData.Items.Count;
            LessonDone = LessonData.Items.Count(i => DoneItems.Contains(i.Id));
        }

        return Page();
    }
}
