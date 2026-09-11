using NoteX.Models;

namespace NoteX.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _id = Guid.NewGuid().ToString();
    private string _title = "新規ページ";
    private string _content = string.Empty;
    private bool _isModified;
    private int _cursorLine = 1;
    private int _cursorColumn = 1;
    private int _charCount;
    private int _selectedCharCount;
    private string _encoding = "utf-8";
    private DateTime _createdAt = DateTime.Now;
    private DateTime _modifiedAt = DateTime.Now;
    private int _caretIndex = 0;
    private double _verticalOffset = 0.0;
    private bool _isEditingTitle;
    private string _editingTitleText = string.Empty;

    public bool IsEditingTitle
    {
        get => _isEditingTitle;
        set => SetProperty(ref _isEditingTitle, value);
    }

    public string EditingTitleText
    {
        get => _editingTitleText;
        set => SetProperty(ref _editingTitleText, value);
    }

    public void StartTitleEditing()
    {
        EditingTitleText = Title;
        IsEditingTitle = true;
    }

    public void CommitTitleEditing()
    {
        if (!IsEditingTitle) return;
        string trimmed = EditingTitleText.Trim();
        if (!string.IsNullOrEmpty(trimmed) && trimmed != Title)
        {
            Title = trimmed;
        }
        IsEditingTitle = false;
    }

    public void CancelTitleEditing()
    {
        EditingTitleText = Title;
        IsEditingTitle = false;
    }

    public int CaretIndex
    {
        get => _caretIndex;
        set => SetProperty(ref _caretIndex, value);
    }

    public double VerticalOffset
    {
        get => _verticalOffset;
        set => SetProperty(ref _verticalOffset, value);
    }

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                IsModified = true;
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                IsModified = true;
                CharCount = value?.Length ?? 0;
            }
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (SetProperty(ref _isModified, value))
            {
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public string DisplayTitle => IsModified ? $"{Title} *" : Title;

    public int CursorLine
    {
        get => _cursorLine;
        set => SetProperty(ref _cursorLine, value);
    }

    public int CursorColumn
    {
        get => _cursorColumn;
        set => SetProperty(ref _cursorColumn, value);
    }

    public int CharCount
    {
        get => _charCount;
        set => SetProperty(ref _charCount, value);
    }

    public int SelectedCharCount
    {
        get => _selectedCharCount;
        set
        {
            if (SetProperty(ref _selectedCharCount, value))
            {
                OnPropertyChanged(nameof(StatsText));
            }
        }
    }

    public string Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public DateTime ModifiedAt
    {
        get => _modifiedAt;
        set => SetProperty(ref _modifiedAt, value);
    }

    public string StatsText
    {
        get
        {
            if (SelectedCharCount > 0)
            {
                return $"{CharCount:N0} 文字 (選択中: {SelectedCharCount:N0})";
            }
            return $"{CharCount:N0} 文字";
        }
    }

    public void UpdateCaretPosition(int line, int column, int selectionLength, int caretIndex = 0, double verticalOffset = 0.0)
    {
        CursorLine = line;
        CursorColumn = column;
        SelectedCharCount = selectionLength;
        CaretIndex = caretIndex;
        VerticalOffset = verticalOffset;
    }

    public void MarkAsSaved()
    {
        IsModified = false;
        ModifiedAt = DateTime.Now;
    }

    public NoteXPage ToModel()
    {
        return new NoteXPage
        {
            Id = Id,
            Title = Title,
            Content = Content,
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            Encoding = Encoding
        };
    }

    public static PageViewModel FromModel(NoteXPage page)
    {
        var vm = new PageViewModel
        {
            Id = page.Id,
            _title = page.Title,
            _content = page.Content ?? string.Empty,
            _encoding = page.Encoding ?? "utf-8",
            _createdAt = page.CreatedAt,
            _modifiedAt = page.ModifiedAt,
            _isModified = false,
            _charCount = page.Content?.Length ?? 0
        };
        return vm;
    }

    public PageViewModel Clone()
    {
        var clone = new PageViewModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"{Title} (コピー)",
            Content = Content,
            Encoding = Encoding,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            IsModified = true
        };
        return clone;
    }
}
