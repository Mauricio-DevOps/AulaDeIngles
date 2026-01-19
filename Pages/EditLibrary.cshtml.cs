using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.IO;
using System;
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
        JsonContent = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Page();
    }

    public IActionResult OnPost()
    {
        var role = HttpContext.Session.GetString("role");
        if (role != "admin")
            return RedirectToPage("/Login");
        Log("OnPost called");
        Log($"Raw JsonContent length: { (JsonContent?.Length ?? 0) }");
        try
        {
            var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
            var previewPath = Path.Combine(logsDir, "last_received.json");
            System.IO.File.WriteAllText(previewPath, JsonContent ?? "");
            Log("Wrote last_received.json for inspection.");
            var preview = (JsonContent ?? "").Length > 500 ? (JsonContent ?? "").Substring(0, 500) + "..." : (JsonContent ?? "");
            Log("JsonContent preview: " + preview.Replace("\r\n", " ").Replace("\n", " "));
        }
        catch (Exception exLog)
        {
            Log("Failed to write preview file: " + exLog.ToString());
        }

        if (string.IsNullOrWhiteSpace(JsonContent))
        {
            Error = "Conteúdo vazio recebido; nenhuma alteração foi salva.";
            Log("JsonContent is empty or whitespace; aborting save.");
            return Page();
        }

        try
        {
            Log("Attempting to deserialize incoming JSON...");
            var content = JsonSerializer.Deserialize<ContentRoot>(JsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (content == null)
            {
                Error = "Conteúdo inválido.";
                Log("Deserialized content is null.");
                return Page();
            }

            content.Modules ??= new List<Module>();
            Log($"Deserialized content.Modules count: {content.Modules.Count}");

            Log("Calling ContentService.Save...");
            try
            {
                ContentService.Save(content);
                Log("ContentService.Save completed successfully.");
            }
            catch (Exception exSave)
            {
                Log("ContentService.Save threw: " + exSave.ToString());
                throw; // rethrow to be caught by outer catch
            }

            return RedirectToPage("/Library");
        }
        catch (Exception ex)
        {
            Error = "Erro ao salvar: " + ex.Message;
            Log("OnPost error: " + ex.ToString());
            return Page();
        }
    }

    private void Log(string message)
    {
        try
        {
            var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
            var logPath = Path.Combine(logsDir, "editlibrary.log");
            var entry = $"[{DateTime.UtcNow:O}] {message.Replace("\r\n", " ").Replace("\n", " ")}" + Environment.NewLine;
            System.IO.File.AppendAllText(logPath, entry);
        }
        catch { /* swallow logging errors to avoid cascading failures */ }
    }
}
