namespace AulaDeIngles.Models;

public class ContentRoot
{
    public List<Module> Modules { get; set; } = new();
}

public class Module
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public List<Lesson> Lessons { get; set; } = new();
}

public class Lesson
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public List<ContentItem> Items { get; set; } = new();
}

public class ContentItem
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = ""; // text | audio | pdf | link
    public string Title { get; set; } = "";
    public string Path { get; set; } = "";
}
