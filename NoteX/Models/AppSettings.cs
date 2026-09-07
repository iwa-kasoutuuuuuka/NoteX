namespace NoteX.Models;

public class AppSettings
{
    public string FontFamily { get; set; } = "Segoe UI Variable Text, Yu Gothic UI, Meiryo UI";
    public double FontSize { get; set; } = 14.0;
    public bool WordWrap { get; set; } = true;
    public bool ShowLineNumbers { get; set; } = true;
    public string Theme { get; set; } = "System"; // "System", "Dark", "Light"
    public double ZoomLevel { get; set; } = 100.0;
    public List<string> RecentFiles { get; set; } = new();
}
