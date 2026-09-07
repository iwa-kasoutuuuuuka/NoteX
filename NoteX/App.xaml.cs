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

        var settings = _settingsService.LoadSettings();
        ApplyTheme(settings.Theme);

        var fileService = new FileService();
        var conversionService = new TextConversionService();
        var dialogService = new WpfDialogService();

        var mainViewModel = new MainViewModel(fileService, conversionService, _settingsService, dialogService);

        // コマンドライン引数でファイルが指定されている場合
        if (e.Args.Length > 0 && File.Exists(e.Args[0]))
        {
            string ext = Path.GetExtension(e.Args[0]).ToLowerInvariant();
            if (ext == ".txtx")
            {
                mainViewModel.LoadDocumentFromPath(e.Args[0]);
            }
            else if (ext == ".txt")
            {
                var imported = conversionService.ImportFiles(new[] { e.Args[0] });
                if (imported.Count > 0)
                {
                    mainViewModel.Pages.Clear();
                    mainViewModel.Pages.Add(PageViewModel.FromModel(imported[0]));
                    mainViewModel.SelectedPage = mainViewModel.Pages[0];
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
