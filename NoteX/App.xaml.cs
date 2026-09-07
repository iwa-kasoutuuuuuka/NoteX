using System.IO;
using System.Windows;
using Microsoft.Win32;
using NoteX.Services;
using NoteX.ViewModels;
using NoteX.Views;

namespace NoteX;

public partial class App : Application
{
    private SettingsService _settingsService = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                File.WriteAllText(logFile, args.Exception.ToString());
                MessageBox.Show($"NoteX の実行中に予期しないエラーが発生しました:\n\n{args.Exception.Message}\n\n詳細ログを保存しました: {logFile}", "NoteX エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                File.WriteAllText(logFile, args.ExceptionObject.ToString());
            }
            catch { }
        };

        var settings = _settingsService.LoadSettings();
        ApplyTheme(settings.Theme);

        var fileService = new FileService();
        var conversionService = new TextConversionService();
        var dialogService = new WpfDialogService();

        var mainViewModel = new MainViewModel(fileService, conversionService, _settingsService, dialogService);

        // コマンドライン引数でファイルが指定されている場合
        if (e.Args.Length > 0)
        {
            var validFiles = e.Args.Where(File.Exists).ToArray();
            string? firstTxtx = validFiles.FirstOrDefault(f => Path.GetExtension(f).Equals(".txtx", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(firstTxtx))
            {
                mainViewModel.LoadDocumentFromPath(firstTxtx);
            }
            else if (validFiles.Length > 0)
            {
                var imported = conversionService.ImportFiles(validFiles);
                if (imported.Count > 0)
                {
                    mainViewModel.Pages.Clear();
                    foreach (var page in imported)
                    {
                        mainViewModel.Pages.Add(PageViewModel.FromModel(page));
                    }
                    mainViewModel.SelectedPage = mainViewModel.Pages[0];
                    mainViewModel.DocumentTitle = Path.GetFileNameWithoutExtension(validFiles[0]);
                    mainViewModel.UpdateWindowTitle();
                    mainViewModel.UpdatePageCountText();
                }
            }
        }

        var mainWindow = new MainWindow(mainViewModel);
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    public static void ApplyTheme(string theme)
    {
        bool isDark = false;
        if (string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase))
        {
            isDark = true;
        }
        else if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
        {
            isDark = false;
        }
        else // "System"
        {
            isDark = CheckSystemUsesDarkTheme();
        }

        string themeUri = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
        var newDict = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };

        var appResources = Current.Resources.MergedDictionaries;
        if (appResources.Count > 0)
        {
            appResources[0] = newDict;
        }
    }

    private static bool CheckSystemUsesDarkTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var val = key.GetValue("AppsUseLightTheme");
                if (val is int lightThemeInt)
                {
                    return lightThemeInt == 0;
                }
            }
        }
        catch
        {
            // レジストリ読み込み不可の場合はライトテーマ
        }
        return false;
    }
}
