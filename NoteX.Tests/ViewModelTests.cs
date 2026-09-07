using System.IO;
using NoteX.Models;
using NoteX.Services;
using NoteX.ViewModels;
using Xunit;

namespace NoteX.Tests;

public class DummyDialogService : IDialogService
{
    public void ShowMessage(string message, string title = "NoteX") { }
    public void ShowError(string message, string title = "エラー") { }
    public ConfirmResult ShowConfirmation(string message, string title = "確認") => ConfirmResult.Yes;
    public string? ShowOpenFileDialog(string filter = "", string defaultExt = "") => null;
    public string[]? ShowOpenFilesDialog(string filter = "", string defaultExt = "") => null;
    public string? ShowSaveFileDialog(string defaultFileName = "", string filter = "", string defaultExt = "") => null;
    public string? ShowFolderBrowserDialog(string description = "") => null;
    public string? ShowInputPrompt(string title, string prompt, string defaultValue = "") => "リネームテスト";
    public ExportOptions? ShowExportDialog(string defaultDirectory) => null;
    public void ShowSettingsDialog(AppSettings settings, SettingsService settingsService) { }
}

public class ViewModelTests
{
    private readonly MainViewModel _vm;

    public ViewModelTests()
    {
        var fileService = new FileService();
        var conversionService = new TextConversionService();
        var settingsService = new SettingsService();
        var dialogService = new DummyDialogService();

        _vm = new MainViewModel(fileService, conversionService, settingsService, dialogService);
    }

    [Fact]
    public void InitialState_HasOneDefaultPage()
    {
        Assert.Single(_vm.Pages);
        Assert.Equal("ページ 1", _vm.Pages[0].Title);
        Assert.Equal(string.Empty, _vm.Pages[0].Content);
        Assert.False(_vm.IsDocumentModified);
        Assert.Equal(_vm.Pages[0], _vm.SelectedPage);
    }

    [Fact]
    public void AddNewPage_IncreasesPageCountAndSelectsIt()
    {
        _vm.AddNewPage();

        Assert.Equal(2, _vm.Pages.Count);
        Assert.Equal("ページ 2", _vm.Pages[1].Title);
        Assert.Equal(_vm.Pages[1], _vm.SelectedPage);
        Assert.True(_vm.IsDocumentModified);
    }

    [Fact]
    public void DuplicatePage_CreatesCopyAdjacentToSelected()
    {
        _vm.SelectedPage!.Title = "オリジナル";
        _vm.SelectedPage.Content = "テスト内容";

        _vm.DuplicatePage(_vm.SelectedPage);

        Assert.Equal(2, _vm.Pages.Count);
        Assert.Equal("オリジナル (コピー)", _vm.Pages[1].Title);
        Assert.Equal("テスト内容", _vm.Pages[1].Content);
        Assert.Equal(_vm.Pages[1], _vm.SelectedPage);
    }

    [Fact]
    public void MovePage_ChangesOrderCorrectly()
    {
        _vm.AddNewPage();
        var page1 = _vm.Pages[0];
        var page2 = _vm.Pages[1];

        // 右へ移動
        _vm.MovePageRight(page1);
        Assert.Equal(page2, _vm.Pages[0]);
        Assert.Equal(page1, _vm.Pages[1]);

        // 左へ移動
        _vm.MovePageLeft(page1);
        Assert.Equal(page1, _vm.Pages[0]);
        Assert.Equal(page2, _vm.Pages[1]);
    }

    [Fact]
    public void ClosePage_RemovesPageAndUpdatesSelected()
    {
        _vm.AddNewPage();
        var page1 = _vm.Pages[0];
        var page2 = _vm.Pages[1];

        _vm.ClosePage(page1);

        Assert.Single(_vm.Pages);
        Assert.Equal(page2, _vm.Pages[0]);
        Assert.Equal(page2, _vm.SelectedPage);
    }

    [Fact]
    public void RenamePage_UpdatesTitleViaDialog()
    {
        var page = _vm.SelectedPage!;
        _vm.RenamePage(page);

        Assert.Equal("リネームテスト", page.Title);
        Assert.True(page.IsModified);
    }

    [Fact]
    public void ContentModification_UpdatesStatsAndModifiedFlag()
    {
        var page = _vm.SelectedPage!;
        page.Content = "こんにちはNoteX";
        page.UpdateCaretPosition(2, 5, 3);

        Assert.Equal(10, page.CharCount);
        Assert.Equal(3, page.SelectedCharCount);
        Assert.Equal("10 文字 (選択中: 3)", page.StatsText);
        Assert.Equal(2, page.CursorLine);
        Assert.Equal(5, page.CursorColumn);
        Assert.True(page.IsModified);
        Assert.True(_vm.IsDocumentModified);
    }

    [Fact]
    public void ZoomCommands_AdjustZoomWithinBounds()
    {
        _vm.Settings.ZoomLevel = 100.0;

        _vm.ZoomIn();
        Assert.Equal(110.0, _vm.Settings.ZoomLevel);

        _vm.ZoomOut();
        Assert.Equal(100.0, _vm.Settings.ZoomLevel);

        _vm.ResetZoom();
        Assert.Equal(100.0, _vm.Settings.ZoomLevel);
    }

    [Fact]
    public void PortableSettings_LoadsAndSavesToSpecifiedLocation()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "NoteX_Portable_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            string settingsFile = Path.Combine(tempDir, "settings.json");
            var service = new SettingsService(settingsFile);

            var settings = service.LoadSettings();
            settings.FontSize = 22.0;
            settings.Theme = "Dark";
            service.SaveSettings(settings);

            Assert.True(File.Exists(settingsFile));

            var service2 = new SettingsService(settingsFile);
            var loaded = service2.LoadSettings();
            Assert.Equal(22.0, loaded.FontSize);
            Assert.Equal("Dark", loaded.Theme);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void NextPage_And_PreviousPage_CyclesCorrectly()
    {
        _vm.AddNewPage(); // 2ページ目
        _vm.AddNewPage(); // 3ページ目
        Assert.Equal(3, _vm.Pages.Count);
        Assert.Equal(_vm.Pages[2], _vm.SelectedPage);

        // NextPage で先頭へラップ
        _vm.NextPage();
        Assert.Equal(_vm.Pages[0], _vm.SelectedPage);

        _vm.NextPage();
        Assert.Equal(_vm.Pages[1], _vm.SelectedPage);

        // PreviousPage
        _vm.PreviousPage();
        Assert.Equal(_vm.Pages[0], _vm.SelectedPage);

        // 先頭から PreviousPage で末尾へラップ
        _vm.PreviousPage();
        Assert.Equal(_vm.Pages[2], _vm.SelectedPage);
    }

    [Theory]
    [InlineData("", 0, 1, 1)]
    [InlineData("abc", 0, 1, 1)]
    [InlineData("abc", 2, 1, 3)]
    [InlineData("line1\r\nline2", 7, 2, 1)] // \r\nの直後(line2の先頭)
    [InlineData("line1\r\nline2", 10, 2, 4)]
    [InlineData("line1\nline2\nline3", 12, 3, 1)] // LF
    public void GetLogicalLineAndColumn_CalculatesAccurately(string text, int caret, int expectedLine, int expectedCol)
    {
        var (line, col) = MainViewModel.GetLogicalLineAndColumn(text, caret);
        Assert.Equal(expectedLine, line);
        Assert.Equal(expectedCol, col);
    }

    [Fact]
    public void StructureModified_TracksTabStructureChanges()
    {
        Assert.False(_vm.IsDocumentModified);

        // タブ追加
        _vm.AddNewPage();
        Assert.True(_vm.IsDocumentModified);

        // 新規作成初期化
        _vm.CreateInitialDocument();
        Assert.False(_vm.IsDocumentModified);
    }

    [Fact]
    public void DynamicNewLineStatus_DetectsCorrectLineEnding()
    {
        _vm.SelectedPage!.Content = "Hello\r\nWorld";
        Assert.Equal("Windows (CRLF)", _vm.StatusNewLineText);

        _vm.SelectedPage.Content = "Hello\nWorld";
        Assert.Equal("Unix (LF)", _vm.StatusNewLineText);
    }

    [Fact]
    public void HandleDroppedFiles_ImportsTxtFilesCorrectly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "NoteX_DropTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string tempFile1 = Path.Combine(tempDir, "file1.txt");
        string tempFile2 = Path.Combine(tempDir, "file2.txt");
        try
        {
            File.WriteAllText(tempFile1, "ファイル1の内容");
            File.WriteAllText(tempFile2, "ファイル2の内容");

            _vm.HandleDroppedFiles(new[] { tempFile1, tempFile2 });

            Assert.Equal(2, _vm.Pages.Count);
            Assert.Contains(_vm.Pages, p => p.Content == "ファイル1の内容");
            Assert.Contains(_vm.Pages, p => p.Content == "ファイル2の内容");
            Assert.True(_vm.IsDocumentModified);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
