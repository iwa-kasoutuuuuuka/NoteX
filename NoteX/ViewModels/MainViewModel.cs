using System.Collections.ObjectModel;
using System.IO;
using NoteX.Models;
using NoteX.Services;

namespace NoteX.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly FileService _fileService;
    private readonly TextConversionService _conversionService;
    private readonly SettingsService _settingsService;
    private readonly IDialogService _dialogService;

    private string? _currentFilePath;
    private string _documentTitle = "無題";
    private PageViewModel? _selectedPage;
    private AppSettings _settings;
    private bool _isFindReplaceVisible;
    private string _findText = string.Empty;
    private string _replaceText = string.Empty;
    private bool _matchCase;
    private Action<string, bool, bool>? _findHandler; // text, matchCase, searchDown
    private Action<string, string, bool, bool>? _replaceHandler; // find, replace, matchCase, replaceAll
    private Timer? _zoomSaveTimer;

    public ObservableCollection<PageViewModel> Pages { get; } = new();

    public AppSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public string? CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (SetProperty(ref _currentFilePath, value))
            {
                UpdateWindowTitle();
            }
        }
    }

    public string DocumentTitle
    {
        get => _documentTitle;
        set
        {
            if (SetProperty(ref _documentTitle, value))
            {
                UpdateWindowTitle();
            }
        }
    }

    public PageViewModel? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (ReferenceEquals(_selectedPage, value)) return;

            if (_selectedPage != null)
            {
                _selectedPage.PropertyChanged -= OnPagePropertyChanged;
            }

            if (SetProperty(ref _selectedPage, value))
            {
                if (_selectedPage != null)
                {
                    _selectedPage.PropertyChanged += OnPagePropertyChanged;
                }
                UpdateStatusStats();
                UpdatePageCountText();
            }
        }
    }

    private string _windowTitle = "無題 - NoteX";
    private bool _isStructureModified = false;

    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetProperty(ref _windowTitle, value);
    }

    public bool IsDocumentModified => _isStructureModified || Pages.Any(p => p.IsModified);

    private string _pageCountText = "ページ 1 / 1";
    public string PageCountText
    {
        get => _pageCountText;
        private set => SetProperty(ref _pageCountText, value);
    }

    private string _statusLineColText = "行 1, 列 1";
    public string StatusLineColText
    {
        get => _statusLineColText;
        private set => SetProperty(ref _statusLineColText, value);
    }

    private string _statusStatsText = "0 文字";
    public string StatusStatsText
    {
        get => _statusStatsText;
        private set => SetProperty(ref _statusStatsText, value);
    }

    public string StatusEncodingText => SelectedPage?.Encoding.ToUpperInvariant() ?? "UTF-8";

    public string StatusNewLineText
    {
        get
        {
            string? content = SelectedPage?.Content;
            if (string.IsNullOrEmpty(content)) return "Windows (CRLF)";
            if (content.Contains("\r\n")) return "Windows (CRLF)";
            if (content.Contains('\n')) return "Unix (LF)";
            if (content.Contains('\r')) return "Macintosh (CR)";
            return "Windows (CRLF)";
        }
    }

    public string StatusZoomText => $"{Settings.ZoomLevel:0}%";

    public bool IsFindReplaceVisible
    {
        get => _isFindReplaceVisible;
        set => SetProperty(ref _isFindReplaceVisible, value);
    }

    public string FindText
    {
        get => _findText;
        set => SetProperty(ref _findText, value);
    }

    public string ReplaceText
    {
        get => _replaceText;
        set => SetProperty(ref _replaceText, value);
    }

    public bool MatchCase
    {
        get => _matchCase;
        set => SetProperty(ref _matchCase, value);
    }

    // Commands
    public RelayCommand NewDocumentCommand { get; }
    public RelayCommand OpenDocumentCommand { get; }
    public RelayCommand SaveDocumentCommand { get; }
    public RelayCommand SaveAsDocumentCommand { get; }
    public RelayCommand<string> OpenRecentFileCommand { get; }

    public RelayCommand AddNewPageCommand { get; }
    public RelayCommand<PageViewModel> ClosePageCommand { get; }
    public RelayCommand<PageViewModel> DuplicatePageCommand { get; }
    public RelayCommand<PageViewModel> MovePageLeftCommand { get; }
    public RelayCommand<PageViewModel> MovePageRightCommand { get; }
    public RelayCommand<PageViewModel> RenamePageCommand { get; }
    public RelayCommand<PageViewModel> StartRenamePageCommand { get; }
    public RelayCommand<PageViewModel> SetFirstLineAsTitleCommand { get; }

    public RelayCommand NextPageCommand { get; }
    public RelayCommand PreviousPageCommand { get; }

    public RelayCommand ExportAllPagesToTxtCommand { get; }
    public RelayCommand ExportCurrentPageToTxtCommand { get; }
    public RelayCommand ExportMergedTxtCommand { get; }
    public RelayCommand ImportTxtFilesCommand { get; }
    public RelayCommand ImportTxtDirectoryCommand { get; }

    public RelayCommand ToggleWordWrapCommand { get; }
    public RelayCommand ToggleLineNumbersCommand { get; }
    public RelayCommand ZoomInCommand { get; }
    public RelayCommand ZoomOutCommand { get; }
    public RelayCommand ResetZoomCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }

    public RelayCommand ShowFindCommand { get; }
    public RelayCommand ShowReplaceCommand { get; }
    public RelayCommand CloseFindReplaceCommand { get; }
    public RelayCommand FindNextCommand { get; }
    public RelayCommand FindPreviousCommand { get; }
    public RelayCommand ReplaceNextCommand { get; }
    public RelayCommand ReplaceAllCommand { get; }

    public MainViewModel(
        FileService fileService,
        TextConversionService conversionService,
        SettingsService settingsService,
        IDialogService dialogService)
    {
        _fileService = fileService;
        _conversionService = conversionService;
        _settingsService = settingsService;
        _dialogService = dialogService;

        _settings = _settingsService.LoadSettings();

        // Commands initialization
        NewDocumentCommand = new RelayCommand(NewDocument);
        OpenDocumentCommand = new RelayCommand(OpenDocument);
        SaveDocumentCommand = new RelayCommand(SaveDocument);
        SaveAsDocumentCommand = new RelayCommand(SaveAsDocument);
        OpenRecentFileCommand = new RelayCommand<string>(OpenRecentFile);

        AddNewPageCommand = new RelayCommand(AddNewPage);
        ClosePageCommand = new RelayCommand<PageViewModel>(ClosePage, p => Pages.Count > 1 || (p != null && (!string.IsNullOrEmpty(p.Content) || p.Title != "ページ 1")));
        DuplicatePageCommand = new RelayCommand<PageViewModel>(DuplicatePage, p => p != null);
        MovePageLeftCommand = new RelayCommand<PageViewModel>(MovePageLeft, p => p != null && Pages.IndexOf(p) > 0);
        MovePageRightCommand = new RelayCommand<PageViewModel>(MovePageRight, p => p != null && Pages.IndexOf(p) < Pages.Count - 1);
        RenamePageCommand = new RelayCommand<PageViewModel>(RenamePage, p => p != null);
        StartRenamePageCommand = new RelayCommand<PageViewModel>(StartRenamePage, p => p != null);
        SetFirstLineAsTitleCommand = new RelayCommand<PageViewModel>(SetFirstLineAsTitle, p => p != null && !string.IsNullOrWhiteSpace(p.Content));

        NextPageCommand = new RelayCommand(NextPage, () => Pages.Count > 1);
        PreviousPageCommand = new RelayCommand(PreviousPage, () => Pages.Count > 1);

        ExportAllPagesToTxtCommand = new RelayCommand(ExportAllPagesToTxt);
        ExportCurrentPageToTxtCommand = new RelayCommand(ExportCurrentPageToTxt, () => SelectedPage != null);
        ExportMergedTxtCommand = new RelayCommand(ExportMergedTxt);
        ImportTxtFilesCommand = new RelayCommand(ImportTxtFiles);
        ImportTxtDirectoryCommand = new RelayCommand(ImportTxtDirectory);

        ToggleWordWrapCommand = new RelayCommand(ToggleWordWrap);
        ToggleLineNumbersCommand = new RelayCommand(ToggleLineNumbers);
        ZoomInCommand = new RelayCommand(ZoomIn);
        ZoomOutCommand = new RelayCommand(ZoomOut);
        ResetZoomCommand = new RelayCommand(ResetZoom);
        OpenSettingsCommand = new RelayCommand(OpenSettings);

        ShowFindCommand = new RelayCommand(() => IsFindReplaceVisible = true);
        ShowReplaceCommand = new RelayCommand(() => IsFindReplaceVisible = true);
        CloseFindReplaceCommand = new RelayCommand(() => IsFindReplaceVisible = false);
        FindNextCommand = new RelayCommand(() => _findHandler?.Invoke(FindText, MatchCase, true), () => !string.IsNullOrEmpty(FindText));
        FindPreviousCommand = new RelayCommand(() => _findHandler?.Invoke(FindText, MatchCase, false), () => !string.IsNullOrEmpty(FindText));
        ReplaceNextCommand = new RelayCommand(() => _replaceHandler?.Invoke(FindText, ReplaceText, MatchCase, false), () => !string.IsNullOrEmpty(FindText));
        ReplaceAllCommand = new RelayCommand(() => _replaceHandler?.Invoke(FindText, ReplaceText, MatchCase, true), () => !string.IsNullOrEmpty(FindText));

        // 初期ドキュメント作成
        CreateInitialDocument();
    }

    public void RegisterFindReplaceHandlers(Action<string, bool, bool> findHandler, Action<string, string, bool, bool> replaceHandler)
    {
        _findHandler = findHandler;
        _replaceHandler = replaceHandler;
    }

    public void CreateInitialDocument()
    {
        Pages.Clear();
        var defaultPage = new PageViewModel
        {
            Title = "ページ 1",
            Content = string.Empty,
            IsModified = false
        };
        Pages.Add(defaultPage);
        SelectedPage = defaultPage;
        CurrentFilePath = null;
        DocumentTitle = "無題";
        _isStructureModified = false;
        UpdateWindowTitle();
    }

    private void OnPagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PageViewModel.IsModified))
        {
            UpdateWindowTitle();
        }
        else if (e.PropertyName == nameof(PageViewModel.CursorLine) ||
                 e.PropertyName == nameof(PageViewModel.CursorColumn) ||
                 e.PropertyName == nameof(PageViewModel.SelectedCharCount) ||
                 e.PropertyName == nameof(PageViewModel.CharCount))
        {
            UpdateStatusStats();
        }
        else if (e.PropertyName == nameof(PageViewModel.Encoding))
        {
            OnPropertyChanged(nameof(StatusEncodingText));
        }
    }

    public void UpdateWindowTitle()
    {
        string modifiedIndicator = IsDocumentModified ? "*" : "";
        string fileName = !string.IsNullOrEmpty(CurrentFilePath)
            ? Path.GetFileName(CurrentFilePath)
            : DocumentTitle;

        WindowTitle = $"{modifiedIndicator}{fileName} - NoteX";
    }

    private void UpdateStatusStats()
    {
        if (SelectedPage != null)
        {
            StatusLineColText = $"行 {SelectedPage.CursorLine}, 列 {SelectedPage.CursorColumn}";
            StatusStatsText = SelectedPage.StatsText;
            OnPropertyChanged(nameof(StatusEncodingText));
        }
        else
        {
            StatusLineColText = "行 1, 列 1";
            StatusStatsText = "0 文字";
        }
    }

    public void UpdatePageCountText()
    {
        if (SelectedPage != null && Pages.Count > 0)
        {
            int index = Pages.IndexOf(SelectedPage) + 1;
            PageCountText = $"ページ {index} / {Pages.Count}";
        }
        else
        {
            PageCountText = "ページ 0 / 0";
        }
    }

    public bool ConfirmSaveIfModified()
    {
        if (!IsDocumentModified) return true;

        string docName = !string.IsNullOrEmpty(CurrentFilePath)
            ? Path.GetFileName(CurrentFilePath)
            : DocumentTitle;

        var result = _dialogService.ShowConfirmation(
            $"'{docName}' への変更を保存しますか？",
            "NoteX - 変更の保存"
        );

        if (result == ConfirmResult.Yes)
        {
            return SaveDocumentInternal();
        }
        return result == ConfirmResult.No;
    }

    public void NewDocument()
    {
        if (!ConfirmSaveIfModified()) return;
        CreateInitialDocument();
    }

    public void OpenDocument()
    {
        if (!ConfirmSaveIfModified()) return;

        string? filePath = _dialogService.ShowOpenFileDialog();
        if (string.IsNullOrEmpty(filePath)) return;

        LoadDocumentFromPath(filePath);
    }

    public void OpenRecentFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _dialogService.ShowError("指定されたファイルが見つかりません。");
            return;
        }

        if (!ConfirmSaveIfModified()) return;
        LoadDocumentFromPath(filePath);
    }

    public void LoadDocumentFromPath(string filePath)
    {
        try
        {
            var doc = _fileService.LoadDocument(filePath);
            Pages.Clear();

            foreach (var p in doc.Pages)
            {
                var vm = PageViewModel.FromModel(p);
                Pages.Add(vm);
            }

            int activeIdx = Math.Clamp(doc.ActivePageIndex, 0, Math.Max(0, Pages.Count - 1));
            SelectedPage = Pages.Count > activeIdx ? Pages[activeIdx] : Pages.FirstOrDefault();

            CurrentFilePath = filePath;
            DocumentTitle = Path.GetFileNameWithoutExtension(filePath);
            _isStructureModified = false;
            UpdateWindowTitle();
            UpdatePageCountText();

            _settingsService.AddRecentFile(_settings, filePath);
            OnPropertyChanged(nameof(Settings));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"ファイルの読み込みに失敗しました:\n{ex.Message}");
        }
    }

    public void SaveDocument()
    {
        SaveDocumentInternal();
    }

    private bool SaveDocumentInternal()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            return SaveAsDocumentInternal();
        }

        try
        {
            var doc = new NoteXDocument
            {
                Title = DocumentTitle,
                ActivePageIndex = SelectedPage != null ? Pages.IndexOf(SelectedPage) : 0,
                Pages = Pages.Select(p => p.ToModel()).ToList()
            };

            _fileService.SaveDocument(doc, CurrentFilePath);

            foreach (var page in Pages)
            {
                page.MarkAsSaved();
            }

            _isStructureModified = false;
            _settingsService.AddRecentFile(_settings, CurrentFilePath);
            UpdateWindowTitle();
            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"ファイルの保存に失敗しました:\n{ex.Message}");
            return false;
        }
    }

    public void SaveAsDocument()
    {
        SaveAsDocumentInternal();
    }

    private bool SaveAsDocumentInternal()
    {
        string defaultName = string.IsNullOrEmpty(DocumentTitle) ? "無題.txtx" : $"{DocumentTitle}.txtx";
        string? targetPath = _dialogService.ShowSaveFileDialog(defaultName);
        if (string.IsNullOrEmpty(targetPath)) return false;

        CurrentFilePath = targetPath;
        DocumentTitle = Path.GetFileNameWithoutExtension(targetPath);
        return SaveDocumentInternal();
    }

    // ページ操作
    public void AddNewPage()
    {
        int nextNum = Pages.Count + 1;
        var newPage = new PageViewModel
        {
            Title = $"ページ {nextNum}",
            Content = string.Empty,
            IsModified = true
        };
        Pages.Add(newPage);
        SelectedPage = newPage;
        _isStructureModified = true;
        UpdatePageCountText();
        UpdateWindowTitle();
    }

    public void ClosePage(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        // 未保存の変更がある場合は確認
        if (page.IsModified && !string.IsNullOrEmpty(page.Content))
        {
            var confirm = _dialogService.ShowConfirmation(
                $"ページ '{page.Title}' には未保存の変更があります。\nこのページを閉じますか？ (変更内容は失われます)",
                "NoteX - ページの確認"
            );
            if (confirm != ConfirmResult.Yes)
            {
                return;
            }
        }

        if (Pages.Count <= 1)
        {
            // 最後の1ページの場合は中身をクリアして初期化
            page.Title = "ページ 1";
            page.Content = string.Empty;
            page.MarkAsSaved();
            _isStructureModified = false;
            UpdateWindowTitle();
            return;
        }

        int index = Pages.IndexOf(page);
        Pages.Remove(page);

        if (SelectedPage == page)
        {
            int newIdx = Math.Min(index, Pages.Count - 1);
            SelectedPage = Pages[newIdx];
        }

        _isStructureModified = true;
        UpdatePageCountText();
        UpdateWindowTitle();
    }

    public void DuplicatePage(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        var clone = page.Clone();
        int index = Pages.IndexOf(page);
        Pages.Insert(index + 1, clone);
        SelectedPage = clone;
        _isStructureModified = true;
        UpdatePageCountText();
        UpdateWindowTitle();
    }

    public void MovePageLeft(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        int index = Pages.IndexOf(page);
        if (index > 0)
        {
            Pages.Move(index, index - 1);
            SelectedPage = page;
            _isStructureModified = true;
            UpdatePageCountText();
            UpdateWindowTitle();
        }
    }

    public void MovePageRight(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        int index = Pages.IndexOf(page);
        if (index < Pages.Count - 1)
        {
            Pages.Move(index, index + 1);
            SelectedPage = page;
            _isStructureModified = true;
            UpdatePageCountText();
            UpdateWindowTitle();
        }
    }

    public void NextPage()
    {
        if (Pages.Count <= 1) return;
        int idx = SelectedPage != null ? Pages.IndexOf(SelectedPage) : -1;
        int nextIdx = (idx + 1) % Pages.Count;
        SelectedPage = Pages[nextIdx];
    }

    public void PreviousPage()
    {
        if (Pages.Count <= 1) return;
        int idx = SelectedPage != null ? Pages.IndexOf(SelectedPage) : -1;
        int prevIdx = idx <= 0 ? Pages.Count - 1 : idx - 1;
        SelectedPage = Pages[prevIdx];
    }

    public void HandleDroppedFiles(string[] files)
    {
        if (files == null || files.Length == 0) return;

        try
        {
            // .txtx ファイルがある場合はそれを開く
            string? txtxFile = files.FirstOrDefault(f => Path.GetExtension(f).Equals(".txtx", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(txtxFile))
            {
                if (!ConfirmSaveIfModified()) return;
                LoadDocumentFromPath(txtxFile);
                return;
            }

            // テキストファイルを抽出
            var txtFiles = files.Where(f =>
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                return ext is ".txt" or ".log" or ".csv" or ".md" or ".json" or ".xaml" or ".cs" or ".xml";
            }).ToArray();

            if (txtFiles.Length > 0)
            {
                var imported = _conversionService.ImportFiles(txtFiles);
                if (imported.Count == 0) return;

                bool replaceFirstEmpty = Pages.Count == 1 && string.IsNullOrEmpty(Pages[0].Content) && Pages[0].Title == "ページ 1";
                if (replaceFirstEmpty)
                {
                    Pages.Clear();
                }

                foreach (var page in imported)
                {
                    var vm = PageViewModel.FromModel(page);
                    vm.IsModified = true;
                    Pages.Add(vm);
                }

                SelectedPage = Pages.LastOrDefault();
                _isStructureModified = true;
                UpdatePageCountText();
                UpdateWindowTitle();
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"ファイルの取り込み中にエラーが発生しました:\n{ex.Message}");
        }
    }

    public static (int Line, int Column) GetLogicalLineAndColumn(string text, int caretIndex)
    {
        if (string.IsNullOrEmpty(text) || caretIndex <= 0)
        {
            return (1, 1);
        }

        int caret = Math.Clamp(caretIndex, 0, text.Length);
        int line = 1;
        int lastLineBreakEnd = 0;

        for (int i = 0; i < caret; i++)
        {
            if (text[i] == '\r')
            {
                line++;
                if (i + 1 < caret && text[i + 1] == '\n')
                {
                    i++;
                }
                lastLineBreakEnd = i + 1;
            }
            else if (text[i] == '\n')
            {
                line++;
                lastLineBreakEnd = i + 1;
            }
        }

        int col = caret - lastLineBreakEnd + 1;
        return (line, col);
    }

    public void StartRenamePage(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        foreach (var p in Pages)
        {
            if (p != page && p.IsEditingTitle)
            {
                p.CommitTitleEditing();
            }
        }
        page.StartTitleEditing();
    }

    public void RenamePage(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        // インライン編集を優先して開始
        StartRenamePage(page);
    }

    public void RenamePageViaDialog(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        string? newTitle = _dialogService.ShowInputPrompt("ページ名の変更", "新しいページ名を入力してください:", page.Title);
        if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != page.Title)
        {
            page.Title = newTitle.Trim();
            _isStructureModified = true;
            UpdateWindowTitle();
        }
    }

    public void SetFirstLineAsTitle(PageViewModel? page)
    {
        page ??= SelectedPage;
        if (page == null) return;

        string content = page.Content ?? string.Empty;
        if (string.IsNullOrWhiteSpace(content)) return;

        using var reader = new StringReader(content);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                if (trimmed.Length > 40)
                {
                    trimmed = trimmed.Substring(0, 40) + "...";
                }
                page.Title = trimmed;
                _isStructureModified = true;
                UpdateWindowTitle();
                break;
            }
        }
    }

    public void ReorderPages(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= Pages.Count || newIndex < 0 || newIndex >= Pages.Count || oldIndex == newIndex)
            return;

        var currentSelected = SelectedPage;
        Pages.Move(oldIndex, newIndex);
        SelectedPage = currentSelected;

        _isStructureModified = true;
        UpdatePageCountText();
        UpdateWindowTitle();
    }

    // txt変換機能
    public void ExportAllPagesToTxt()
    {
        string defaultDir = !string.IsNullOrEmpty(CurrentFilePath)
            ? Path.GetDirectoryName(CurrentFilePath) ?? string.Empty
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var options = _dialogService.ShowExportDialog(defaultDir);
        if (options == null || string.IsNullOrWhiteSpace(options.OutputDirectory)) return;

        var pageModels = Pages.Select(p => p.ToModel()).ToList();
        var result = _conversionService.ExportPagesToDirectory(pageModels, options);

        if (result.Errors.Count == 0)
        {
            _dialogService.ShowMessage($"全 {result.ExportedCount} ページを以下のフォルダに出力しました:\n{options.OutputDirectory}", "一括エクスポート完了");
        }
        else
        {
            _dialogService.ShowError($"一部のファイル出力でエラーが発生しました:\n{string.Join("\n", result.Errors)}");
        }
    }

    public void ExportCurrentPageToTxt()
    {
        if (SelectedPage == null) return;

        string safeName = TextConversionService.SanitizeFileName(SelectedPage.Title);
        string? targetPath = _dialogService.ShowSaveFileDialog(
            $"{safeName}.txt",
            "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
            ".txt"
        );
        if (string.IsNullOrEmpty(targetPath)) return;

        try
        {
            _conversionService.ExportSinglePage(SelectedPage.ToModel(), targetPath);
            _dialogService.ShowMessage($"現在のページをテキストファイルとして書き出しました:\n{targetPath}", "エクスポート完了");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"エクスポートに失敗しました:\n{ex.Message}");
        }
    }

    public void ExportMergedTxt()
    {
        string safeDocName = TextConversionService.SanitizeFileName(DocumentTitle);
        string? targetPath = _dialogService.ShowSaveFileDialog(
            $"{safeDocName}_全ページ結合.txt",
            "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
            ".txt"
        );
        if (string.IsNullOrEmpty(targetPath)) return;

        try
        {
            var doc = new NoteXDocument
            {
                Title = DocumentTitle,
                Pages = Pages.Select(p => p.ToModel()).ToList()
            };
            _conversionService.ExportMergedDocument(doc, targetPath);
            _dialogService.ShowMessage($"全ページを1つのテキストファイルに結合して書き出しました:\n{targetPath}", "結合エクスポート完了");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"結合エクスポートに失敗しました:\n{ex.Message}");
        }
    }

    public void ImportTxtFiles()
    {
        string[]? files = _dialogService.ShowOpenFilesDialog(
            "テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
            ".txt"
        );
        if (files == null || files.Length == 0) return;

        try
        {
            var imported = _conversionService.ImportFiles(files);
            if (imported.Count == 0) return;

            // 現在のページが空の1ページのみの場合、それを置き換えるか確認
            bool replaceFirstEmpty = Pages.Count == 1 && string.IsNullOrEmpty(Pages[0].Content) && Pages[0].Title == "ページ 1";
            if (replaceFirstEmpty)
            {
                Pages.Clear();
            }

            foreach (var page in imported)
            {
                var vm = PageViewModel.FromModel(page);
                vm.IsModified = true;
                Pages.Add(vm);
            }

            SelectedPage = Pages.LastOrDefault();
            _isStructureModified = true;
            UpdatePageCountText();
            UpdateWindowTitle();
            _dialogService.ShowMessage($"{imported.Count} 個のテキストファイルを新しいページとして取り込みました。", "インポート完了");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"インポート中にエラーが発生しました:\n{ex.Message}");
        }
    }

    public void ImportTxtDirectory()
    {
        string? folder = _dialogService.ShowFolderBrowserDialog("テキストファイルを取り込むフォルダを選択");
        if (string.IsNullOrEmpty(folder)) return;

        try
        {
            var imported = _conversionService.ImportDirectory(folder);
            if (imported.Count == 0)
            {
                _dialogService.ShowMessage("選択したフォルダに .txt ファイルが見つかりませんでした。", "インポート");
                return;
            }

            bool replaceFirstEmpty = Pages.Count == 1 && string.IsNullOrEmpty(Pages[0].Content) && Pages[0].Title == "ページ 1";
            if (replaceFirstEmpty)
            {
                Pages.Clear();
            }

            foreach (var page in imported)
            {
                var vm = PageViewModel.FromModel(page);
                vm.IsModified = true;
                Pages.Add(vm);
            }

            SelectedPage = Pages.LastOrDefault();
            _isStructureModified = true;
            UpdatePageCountText();
            UpdateWindowTitle();
            _dialogService.ShowMessage($"{imported.Count} 個のテキストファイルを取り込みました。", "インポート完了");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"フォルダ取り込み中にエラーが発生しました:\n{ex.Message}");
        }
    }

    // 表示設定
    public void ToggleWordWrap()
    {
        Settings.WordWrap = !Settings.WordWrap;
        _settingsService.SaveSettings(Settings);
        OnPropertyChanged(nameof(Settings));
    }

    public void ToggleLineNumbersCommandExecute()
    {
        Settings.ShowLineNumbers = !Settings.ShowLineNumbers;
        _settingsService.SaveSettings(Settings);
        OnPropertyChanged(nameof(Settings));
    }

    private void ToggleLineNumbers() => ToggleLineNumbersCommandExecute();

    private void RequestDebouncedSettingsSave()
    {
        _zoomSaveTimer?.Dispose();
        _zoomSaveTimer = new Timer(_ =>
        {
            _settingsService.SaveSettings(Settings);
        }, null, 500, Timeout.Infinite);
    }

    public void ZoomIn()
    {
        if (Settings.ZoomLevel < 500)
        {
            Settings.ZoomLevel = Math.Min(500, Settings.ZoomLevel + 10);
            RequestDebouncedSettingsSave();
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(StatusZoomText));
        }
    }

    public void ZoomOut()
    {
        if (Settings.ZoomLevel > 20)
        {
            Settings.ZoomLevel = Math.Max(20, Settings.ZoomLevel - 10);
            RequestDebouncedSettingsSave();
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(StatusZoomText));
        }
    }

    public void ResetZoom()
    {
        Settings.ZoomLevel = 100.0;
        _zoomSaveTimer?.Dispose();
        _settingsService.SaveSettings(Settings);
        OnPropertyChanged(nameof(Settings));
        OnPropertyChanged(nameof(StatusZoomText));
    }

    public void OpenSettings()
    {
        _dialogService.ShowSettingsDialog(Settings, _settingsService);
        OnPropertyChanged(nameof(Settings));
    }
}
