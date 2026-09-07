using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NoteX.Services;

namespace NoteX.Views;

public partial class ExportDialog : Window
{
    public ExportOptions Options { get; private set; }

    public ExportDialog(string defaultDirectory = "")
    {
        InitializeComponent();

        if (string.IsNullOrEmpty(defaultDirectory))
        {
            defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        FolderPathTextBox.Text = defaultDirectory;
        Options = new ExportOptions { OutputDirectory = defaultDirectory };
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "テキスト出力先フォルダを選択",
            InitialDirectory = FolderPathTextBox.Text
        };

        if (dialog.ShowDialog(this) == true)
        {
            FolderPathTextBox.Text = dialog.FolderName;
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        string dir = FolderPathTextBox.Text.Trim();
        if (string.IsNullOrEmpty(dir))
        {
            MessageBox.Show(this, "出力先フォルダを指定してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string encoding = (EncodingComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "utf-8";
        string newLine = (NewLineComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "\r\n";

        Options = new ExportOptions
        {
            OutputDirectory = dir,
            IncludePageNumberPrefix = NumberPrefixCheckBox.IsChecked == true,
            OverwriteExisting = OverwriteCheckBox.IsChecked == true,
            EncodingName = encoding,
            NewLine = newLine
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
