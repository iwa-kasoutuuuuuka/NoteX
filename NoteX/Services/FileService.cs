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

    private const long MaxAllowedFileSize = 50 * 1024 * 1024; // 50MB

    public NoteXDocument LoadDocument(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"ファイルが見つかりません: {filePath}");
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxAllowedFileSize)
        {
            throw new InvalidDataException($"ファイルサイズが大きすぎます ({fileInfo.Length / (1024 * 1024)}MB)。50MB以下のファイルを指定してください。");
        }

        if (fileInfo.Length == 0)
        {
            // 空ファイルの場合は安全にデフォルトドキュメントを生成
            return NoteXDocument.CreateDefault(Path.GetFileNameWithoutExtension(filePath));
        }

        string json = File.ReadAllText(filePath);
        var document = JsonSerializer.Deserialize<NoteXDocument>(json, JsonOptions);

        if (document == null)
        {
            throw new InvalidDataException("ファイルの解析に失敗しました。形式が正しいか確認してください。");
        }

        // ページリストのnullチェック
        document.Pages ??= new List<NoteXPage>();

        // ページが空の場合は最低1ページ作成
        if (document.Pages.Count == 0)
        {
            document.Pages.Add(new NoteXPage { Title = "ページ 1", Content = string.Empty });
        }

        // ページ内部のnullサニタイズ
        foreach (var p in document.Pages)
        {
            p.Title = string.IsNullOrWhiteSpace(p.Title) ? "新規ページ" : p.Title;
            p.Content ??= string.Empty;
            p.Encoding = string.IsNullOrWhiteSpace(p.Encoding) ? "utf-8" : p.Encoding;
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

        // 安全なアトミック書き込み (一時ファイル作成 -> 置換 -> 失敗時クリーンアップ)
        string tempFilePath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
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
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
        }
    }
}
