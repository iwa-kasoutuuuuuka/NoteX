using System.IO;
using System.Text.Json;
using NoteX.Models;

namespace NoteX.Services;

public class SettingsService
{
    private string _settingsFilePath;
    private readonly bool _isCustomPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(string? customSettingsPath = null)
    {
        if (!string.IsNullOrEmpty(customSettingsPath))
        {
            _settingsFilePath = customSettingsPath;
            _isCustomPath = true;
            return;
        }

        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string localSettingsFile = Path.Combine(exeDir, "settings.json");
        string portableMarker = Path.Combine(exeDir, "portable.txt");

        // exeと同階層に settings.json または portable.txt がある場合は完全ローカル・ポータブルモードで動作
        if (File.Exists(localSettingsFile) || File.Exists(portableMarker))
        {
            _settingsFilePath = localSettingsFile;
        }
        else
        {
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NoteX"
            );
            _settingsFilePath = Path.Combine(appDataDir, "settings.json");
        }
    }

    public static string GetSafeAppDataDirectory()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NoteX");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings != null) return settings;
            }
            else if (!_isCustomPath)
            {
                // AppData側のフォールバックも確認
                string appDataFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NoteX",
                    "settings.json"
                );
                if (File.Exists(appDataFile))
                {
                    string json = File.ReadAllText(appDataFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (settings != null)
                    {
                        _settingsFilePath = appDataFile;
                        return settings;
                    }
                }
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
            string? dir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (UnauthorizedAccessException) when (!_isCustomPath)
        {
            // 書き込み権限がない場合（Program Files配置時等）、%APPDATA%\NoteX\settings.json にフォールバック
            try
            {
                string appDataDir = GetSafeAppDataDirectory();
                _settingsFilePath = Path.Combine(appDataDir, "settings.json");
                string json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // フォールバック失敗時はアプリ中断させない
            }
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
