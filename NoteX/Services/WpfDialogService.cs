using System.Windows;
using Microsoft.Win32;
using NoteX.Models;
using NoteX.Views;

namespace NoteX.Services;

public class WpfDialogService : IDialogService
{
    public void ShowMessage(string message, string title = "NoteX")
    {
        MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "エラー")
    {
        MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public ConfirmResult ShowConfirmation(string message, string title = "確認")
    {
        var result = MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        return result switch
        {
            MessageBoxResult.Yes => ConfirmResult.Yes,
            MessageBoxResult.No => ConfirmResult.No,
            _ => ConfirmResult.Cancel
        };
    }

    public string? ShowOpenFileDialog(string filter = "NoteX ファイル (*.txtx)|*.txtx|すべてのファイル (*.*)|*.*", string defaultExt = ".txtx")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExt,
            CheckFileExists = true
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }

    public string[]? ShowOpenFilesDialog(string filter = "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*", string defaultExt = ".txt")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExt,
            CheckFileExists = true,
            Multiselect = true
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileNames : null;
    }

    public string? ShowSaveFileDialog(string defaultFileName, string filter = "NoteX ファイル (*.txtx)|*.txtx|すべてのファイル (*.*)|*.*", string defaultExt = ".txtx")
    {
        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = filter,
            DefaultExt = defaultExt,
            OverwritePrompt = true
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FileName : null;
    }

    public string? ShowFolderBrowserDialog(string description = "フォルダを選択してください")
    {
        var dialog = new OpenFolderDialog
        {
            Title = description,
            Multiselect = false
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FolderName : null;
    }

    public string? ShowInputPrompt(string title, string prompt, string defaultValue = "")
    {
        var window = new InputPromptWindow(title, prompt, defaultValue)
        {
            Owner = Application.Current.MainWindow
        };

        return window.ShowDialog() == true ? window.InputText : null;
    }

    public ExportOptions? ShowExportDialog(string defaultDirectory)
    {
        var window = new ExportDialog(defaultDirectory)
        {
            Owner = Application.Current.MainWindow
        };

        return window.ShowDialog() == true ? window.Options : null;
    }

    public void ShowSettingsDialog(AppSettings settings, SettingsService settingsService)
    {
        var window = new SettingsWindow(settings, settingsService)
        {
            Owner = Application.Current.MainWindow
        };

        window.ShowDialog();
    }
}
