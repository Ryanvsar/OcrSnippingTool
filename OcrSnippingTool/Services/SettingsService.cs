using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using OcrSnippingTool.Models;

namespace OcrSnippingTool.Services;

public class SettingsService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "OcrSnippingTool");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private const string AutoStartValueName = "OcrSnippingTool";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Current { get; set; } = new();

    public void Load()
    {
        if (File.Exists(SettingsPath))
        {
            try
            {
                var json = File.ReadAllText(SettingsPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }

        // Sync AutoStart from registry (registry is source of truth)
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        Current.AutoStart = key?.GetValue(AutoStartValueName) != null;
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(Current, JsonOptions);
        File.WriteAllText(SettingsPath, json);
        ApplyAutoStart(Current.AutoStart);
    }

    private static void ApplyAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key == null) return;

        if (enable)
        {
            var exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (exePath != null)
                key.SetValue(AutoStartValueName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AutoStartValueName, throwOnMissingValue: false);
        }
    }
}
