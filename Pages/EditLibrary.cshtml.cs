using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
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

            // Process uploaded files and delete flags
            try
            {
                var files = Request.Form.Files;
                foreach (var module in content.Modules)
                {
                    foreach (var lesson in module.Lessons ?? new List<Lesson>())
                    {
                        foreach (var item in lesson.Items ?? new List<ContentItem>())
                        {
                            var fileKey = "file_" + item.Id;
                            var delKey = "delete_" + item.Id;

                            var file = files.FirstOrDefault(f => f.Name == fileKey);
                            if (file != null && file.Length > 0)
                            {
                                var ext = Path.GetExtension(file.FileName);
                                var folder = item.Type?.ToLower() switch
                                {
                                    "text" => "texts",
                                    "audio" => "audios",
                                    "pdf" => "pdfs",
                                    "video" => "videos",
                                    "image" => "images",
                                    _ => "files"
                                };

                                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                                var safeName = item.Id + (string.IsNullOrEmpty(ext) ? "" : ext);
                                var savePath = Path.Combine(saveDir, safeName);
                                using (var stream = System.IO.File.Create(savePath))
                                {
                                    file.CopyTo(stream);
                                }

                                item.Path = "/" + Path.Combine(folder, safeName).Replace("\\", "/");
                                Log($"Uploaded file for item {item.Id} -> {item.Path}");
                            }

                            if (!string.IsNullOrEmpty(Request.Form[delKey]))
                            {
                                // delete existing file if it's inside wwwroot
                                if (!string.IsNullOrEmpty(item.Path) && item.Path.StartsWith("/"))
                                {
                                    var phys = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.Path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                    try
                                    {
                                        if (System.IO.File.Exists(phys))
                                        {
                                            System.IO.File.Delete(phys);
                                            Log($"Deleted file {phys} for item {item.Id}");
                                        }
                                    }
                                    catch (Exception dex)
                                    {
                                        Log($"Error deleting file {phys}: " + dex.ToString());
                                    }
                                }
                                item.Path = "";
                            }
                        }
                    }
                }
            }
            catch (Exception exFiles)
            {
                Log("Error processing uploaded files: " + exFiles.ToString());
            }

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
