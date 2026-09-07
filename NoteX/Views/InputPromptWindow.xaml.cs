using System.Windows;
using System.Windows.Input;

namespace NoteX.Views;

public partial class InputPromptWindow : Window
{
    public string InputText => InputTextBox.Text;

    public InputPromptWindow(string title, string prompt, string defaultValue = "")
    {
        InitializeComponent();
        Title = title;
        PromptLabel.Text = prompt;
        InputTextBox.Text = defaultValue;
        InputTextBox.SelectAll();
        Loaded += (_, _) => InputTextBox.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.ImeProcessed) return;

        if (e.Key == Key.Enter)
        {
            DialogResult = true;
            Close();
        }
        else if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
