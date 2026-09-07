using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NoteX.Models;

namespace NoteX.Services;

public class FileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true
    };

    public NoteXDocument LoadDocument(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"ファイルが見つかりません: {filePath}");
        }

        string json = File.ReadAllText(filePath);
        var document = JsonSerializer.Deserialize<NoteXDocument>(json, JsonOptions);

        if (document == null)
        {
            throw new InvalidDataException("ファイルの解析に失敗しました。");
        }

        // ページが空の場合は最低1ページ作成
        if (document.Pages == null || document.Pages.Count == 0)
        {
            document.Pages = new List<NoteXPage>
            {
                new NoteXPage { Title = "ページ 1", Content = string.Empty }
            };
        }

        // ドキュメントタイトルが空の場合はファイル名から取得
        if (string.IsNullOrWhiteSpace(document.Title))
        {
            document.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        return document;
    }

    public void SaveDocument(NoteXDocument document, string filePath)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        document.UpdatedAt = DateTime.Now;
        if (string.IsNullOrWhiteSpace(document.Title))
        {
            document.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(document, JsonOptions);

        // 一時ファイルに書き出してから置換（安全なアトミック保存）
        string tempFilePath = filePath + ".tmp";
        File.WriteAllText(tempFilePath, json, System.Text.Encoding.UTF8);

        if (File.Exists(filePath))
        {
            File.Replace(tempFilePath, filePath, null);
        }
        else
        {
            File.Move(tempFilePath, filePath);
        }
    }
}
