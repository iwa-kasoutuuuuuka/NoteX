using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NoteX.Models;
using NoteX.Services;

namespace NoteX.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly SettingsService _settingsService;
    private bool _initialized;

    public SettingsWindow(AppSettings settings, SettingsService settingsService)
    {
        InitializeComponent();
        _settings = settings;
        _settingsService = settingsService;

        PopulateFontFamilies();
        LoadCurrentSettings();
        _initialized = true;
    }

    private void PopulateFontFamilies()
    {
        var commonFonts = new[] { "Segoe UI Variable Text", "Segoe UI", "Yu Gothic UI", "Meiryo UI", "BIZ UDゴシック", "MS Gothic", "Consolas", "Cascadia Code" };
        var installedFonts = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();

        // よく使うフォントを先頭に
        foreach (var font in commonFonts.Reverse())
        {
            if (installedFonts.Contains(font))
            {
                installedFonts.Remove(font);
                installedFonts.Insert(0, font);
            }
        }

        FontFamilyComboBox.ItemsSource = installedFonts;
    }

    private void LoadCurrentSettings()
    {
        // テーマ
        foreach (ComboBoxItem item in ThemeComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), _settings.Theme, StringComparison.OrdinalIgnoreCase))
            {
                ThemeComboBox.SelectedItem = item;
                break;
            }
        }
        if (ThemeComboBox.SelectedItem == null)
            ThemeComboBox.SelectedIndex = 0;

        // フォント
        string currentFont = _settings.FontFamily.Split(',')[0].Trim();
        FontFamilyComboBox.SelectedItem = currentFont;
        if (FontFamilyComboBox.SelectedItem == null)
            FontFamilyComboBox.SelectedIndex = 0;

        // フォントサイズ
        string currentSize = _settings.FontSize.ToString("0");
        bool sizeFound = false;
        foreach (ComboBoxItem item in FontSizeComboBox.Items)
        {
            if (item.Content.ToString() == currentSize)
            {
                FontSizeComboBox.SelectedItem = item;
                sizeFound = true;
                break;
            }
        }
        if (!sizeFound)
        {
            FontSizeComboBox.Text = currentSize;
        }

        // 折り返し・行番号
        WordWrapCheckBox.IsChecked = _settings.WordWrap;
        LineNumbersCheckBox.IsChecked = _settings.ShowLineNumbers;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized) return;

        if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string themeTag)
        {
            _settings.Theme = themeTag;
            App.ApplyTheme(themeTag);
            _settingsService.SaveSettings(_settings);
        }
    }

    private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized) return;

        if (FontFamilyComboBox.SelectedItem is string fontName)
        {
            _settings.FontFamily = fontName;
            _settingsService.SaveSettings(_settings);
        }
    }

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initialized) return;

        if (FontSizeComboBox.SelectedItem is ComboBoxItem item &&
            double.TryParse(item.Content?.ToString(), out double size))
        {
            ApplyFontSize(size);
        }
    }

    private void FontSizeComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;

        if (double.TryParse(FontSizeComboBox.Text.Trim(), out double size) && size >= 6 && size <= 120)
        {
            ApplyFontSize(size);
        }
    }

    private void ApplyFontSize(double size)
    {
        if (size >= 6 && size <= 120 && Math.Abs(_settings.FontSize - size) > 0.1)
        {
            _settings.FontSize = size;
            _settingsService.SaveSettings(_settings);
        }
    }

    private void Setting_Changed(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;

        _settings.WordWrap = WordWrapCheckBox.IsChecked == true;
        _settings.ShowLineNumbers = LineNumbersCheckBox.IsChecked == true;
        _settingsService.SaveSettings(_settings);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
