using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NoteX.ViewModels;

namespace NoteX;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private ScrollViewer? _editorScrollViewer;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.RegisterFindReplaceHandlers(ExecuteFind, ExecuteReplace);

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainEditorTextBox.Focus();

        // TextBox内部のScrollViewerを取得して行番号マージンとスクロール同期
        _editorScrollViewer = FindVisualChild<ScrollViewer>(MainEditorTextBox);
        if (_editorScrollViewer != null)
        {
            _editorScrollViewer.ScrollChanged += EditorScrollViewer_ScrollChanged;
        }

        UpdateLineNumbers();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.ConfirmSaveIfModified())
        {
            e.Cancel = true;
        }
    }

    private void EditorScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_editorScrollViewer != null)
        {
            LineNumberScrollViewer.ScrollToVerticalOffset(_editorScrollViewer.VerticalOffset);
        }
    }

    private void MainEditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateLineNumbers();
    }

    private void UpdateLineNumbers()
    {
        int lineCount = MainEditorTextBox.LineCount;
        if (lineCount < 1) lineCount = 1;

        var sb = new StringBuilder();
        for (int i = 1; i <= lineCount; i++)
        {
            sb.AppendLine(i.ToString());
        }
        LineNumberTextBlock.Text = sb.ToString();
    }

    private void MainEditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedPage == null) return;

        int caret = MainEditorTextBox.CaretIndex;
        int lineIndex = MainEditorTextBox.GetLineIndexFromCharacterIndex(caret);
        if (lineIndex < 0) lineIndex = 0;

        int lineStart = MainEditorTextBox.GetCharacterIndexFromLineIndex(lineIndex);
        int col = caret - lineStart + 1;
        int line = lineIndex + 1;

        _viewModel.SelectedPage.UpdateCaretPosition(line, col, MainEditorTextBox.SelectionLength);
    }

    private void MainEditorTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Delta > 0)
            {
                _viewModel.ZoomIn();
            }
            else if (e.Delta < 0)
            {
                _viewModel.ZoomOut();
            }
            e.Handled = true;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _viewModel.IsFindReplaceVisible)
        {
            _viewModel.IsFindReplaceVisible = false;
            MainEditorTextBox.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.F2 && _viewModel.SelectedPage != null)
        {
            _viewModel.RenamePage(_viewModel.SelectedPage);
            e.Handled = true;
        }
    }

    private void FindInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                _viewModel.FindPreviousCommand.Execute(null);
            }
            else
            {
                _viewModel.FindNextCommand.Execute(null);
            }
            e.Handled = true;
        }
    }

    // 検索処理
    private void ExecuteFind(string searchText, bool matchCase, bool searchDown)
    {
        if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(MainEditorTextBox.Text))
            return;

        string content = MainEditorTextBox.Text;
        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        int startIndex;
        if (searchDown)
        {
            startIndex = MainEditorTextBox.SelectionStart + MainEditorTextBox.SelectionLength;
            if (startIndex >= content.Length) startIndex = 0;

            int foundIdx = content.IndexOf(searchText, startIndex, comparison);
            if (foundIdx < 0 && startIndex > 0)
            {
                // 先頭からループ検索
                foundIdx = content.IndexOf(searchText, 0, startIndex, comparison);
            }

            if (foundIdx >= 0)
            {
                MainEditorTextBox.Select(foundIdx, searchText.Length);
                MainEditorTextBox.Focus();
                ScrollToCaret();
            }
            else
            {
                MessageBox.Show(this, $"\"{searchText}\" が見つかりませんでした。", "NoteX 検索", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            startIndex = MainEditorTextBox.SelectionStart - 1;
            if (startIndex < 0) startIndex = content.Length - 1;

            int foundIdx = content.LastIndexOf(searchText, startIndex, comparison);
            if (foundIdx < 0 && startIndex < content.Length - 1)
            {
                // 末尾からループ検索
                foundIdx = content.LastIndexOf(searchText, content.Length - 1, comparison);
            }

            if (foundIdx >= 0)
            {
                MainEditorTextBox.Select(foundIdx, searchText.Length);
                MainEditorTextBox.Focus();
                ScrollToCaret();
            }
            else
            {
                MessageBox.Show(this, $"\"{searchText}\" が見つかりませんでした。", "NoteX 検索", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    // 置換処理
    private void ExecuteReplace(string searchText, string replaceText, bool matchCase, bool replaceAll)
    {
        if (string.IsNullOrEmpty(searchText)) return;

        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (replaceAll)
        {
            string content = MainEditorTextBox.Text;
            int count = 0;
            var sb = new StringBuilder();
            int lastIndex = 0;

            int foundIndex = content.IndexOf(searchText, 0, comparison);
            while (foundIndex >= 0)
            {
                sb.Append(content.AsSpan(lastIndex, foundIndex - lastIndex));
                sb.Append(replaceText);
                lastIndex = foundIndex + searchText.Length;
                count++;
                foundIndex = content.IndexOf(searchText, lastIndex, comparison);
            }
            sb.Append(content.AsSpan(lastIndex));

            if (count > 0)
            {
                MainEditorTextBox.Text = sb.ToString();
                MessageBox.Show(this, $"{count} 箇所を置換しました。", "NoteX 置換", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, $"\"{searchText}\" が見つかりませんでした。", "NoteX 置換", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return;
        }

        // 単一置換
        if (string.Equals(MainEditorTextBox.SelectedText, searchText, comparison))
        {
            int start = MainEditorTextBox.SelectionStart;
            MainEditorTextBox.SelectedText = replaceText;
            MainEditorTextBox.Select(start + replaceText.Length, 0);
        }

        ExecuteFind(searchText, matchCase, true);
    }

    private void ScrollToCaret()
    {
        int line = MainEditorTextBox.GetLineIndexFromCharacterIndex(MainEditorTextBox.CaretIndex);
        if (line >= 0)
        {
            MainEditorTextBox.ScrollToLine(line);
        }
    }

    // タブのダブルクリックで名前変更
    private void TabTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement element && element.DataContext is PageViewModel page)
        {
            _viewModel.RenamePage(page);
            e.Handled = true;
        }
    }

    // メニューバーハンドラ
    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();
    private void MenuUndo_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.Undo();
    private void MenuRedo_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.Redo();
    private void MenuCut_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.Cut();
    private void MenuCopy_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.Copy();
    private void MenuPaste_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.Paste();
    private void MenuSelectAll_Click(object sender, RoutedEventArgs e) => MainEditorTextBox.SelectAll();

    private void MenuInsertDateTime_Click(object sender, RoutedEventArgs e)
    {
        string nowStr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        MainEditorTextBox.SelectedText = nowStr;
        MainEditorTextBox.CaretIndex += nowStr.Length;
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        string message = "NoteX (ノート・エックス) バージョン 1.0\n\n" +
                         "Windows 11 メモ帳ライクな操作感で、\n" +
                         "1つのファイル (.txtx) 内に複数ページのテキストを保持できるエディタです。\n\n" +
                         "・専用拡張子: .txtx (UTF-8 JSON)\n" +
                         "・txt変換機能: 全ページ一括出力 / 単一出力 / 結合出力 / txtインポート\n\n" +
                         "© 2026 NoteX Project";
        MessageBox.Show(this, message, "NoteX について", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;

            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }
        return null;
    }
}