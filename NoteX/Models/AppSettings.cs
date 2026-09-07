using NoteX.ViewModels;

namespace NoteX.Models;

public class AppSettings : ViewModelBase
{
    private string _fontFamily = "Segoe UI Variable Text, Yu Gothic UI, Meiryo UI";
    private double _fontSize = 14.0;
    private bool _wordWrap = true;
    private bool _showLineNumbers = true;
    private string _theme = "System"; // "System", "Dark", "Light"
    private double _zoomLevel = 100.0;
    private List<string> _recentFiles = new();

    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    public bool WordWrap
    {
        get => _wordWrap;
        set => SetProperty(ref _wordWrap, value);
    }

    public bool ShowLineNumbers
    {
        get => _showLineNumbers;
        set => SetProperty(ref _showLineNumbers, value);
    }

    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, value);
    }

    public List<string> RecentFiles
    {
        get => _recentFiles;
        set => SetProperty(ref _recentFiles, value);
    }
}
