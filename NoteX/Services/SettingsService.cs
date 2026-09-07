using System.IO;
using System.Text.Json;
using NoteX.Models;

namespace NoteX.Services;

public class SettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NoteX"
    );

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings != null) return settings;
            }
        }
        catch
        {
            // エラー時はデフォルト設定を使用
        }

        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch
        {
            // 設定保存エラーはアプリ中断させない
        }
    }

    public void AddRecentFile(AppSettings settings, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        settings.RecentFiles.RemoveAll(p => string.Equals(p, filePath, StringComparison.OrdinalIgnoreCase));
        settings.RecentFiles.Insert(0, filePath);

        // 最大10件
        if (settings.RecentFiles.Count > 10)
        {
            settings.RecentFiles.RemoveRange(10, settings.RecentFiles.Count - 10);
        }

        SaveSettings(settings);
    }
}
