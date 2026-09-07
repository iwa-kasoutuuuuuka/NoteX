using NoteX.Models;
using NoteX.Services;

namespace NoteX.Services;

public enum ConfirmResult
{
    Yes,
    No,
    Cancel
}

public interface IDialogService
{
    void ShowMessage(string message, string title = "NoteX");
    void ShowError(string message, string title = "エラー");
    ConfirmResult ShowConfirmation(string message, string title = "確認");
    string? ShowOpenFileDialog(string filter = "NoteX ファイル (*.txtx)|*.txtx|すべてのファイル (*.*)|*.*", string defaultExt = ".txtx");
    string[]? ShowOpenFilesDialog(string filter = "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*", string defaultExt = ".txt");
    string? ShowSaveFileDialog(string defaultFileName, string filter = "NoteX ファイル (*.txtx)|*.txtx|すべてのファイル (*.*)|*.*", string defaultExt = ".txtx");
    string? ShowFolderBrowserDialog(string description = "フォルダを選択してください");
    string? ShowInputPrompt(string title, string prompt, string defaultValue = "");
    ExportOptions? ShowExportDialog(string defaultDirectory);
    void ShowSettingsDialog(AppSettings settings, SettingsService settingsService);
}
