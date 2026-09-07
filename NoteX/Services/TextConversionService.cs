using System.IO;
using System.Text;
using NoteX.Models;

namespace NoteX.Services;

public class ExportOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public bool IncludePageNumberPrefix { get; set; } = true;
    public string EncodingName { get; set; } = "utf-8";
    public string NewLine { get; set; } = "\r\n"; // CRLF (Windows) or LF (Unix)
    public bool OverwriteExisting { get; set; } = true;
}

public class ExportResult
{
    public int ExportedCount { get; set; }
    public List<string> ExportedFiles { get; set; } = new();
    public List<string> SkippedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class TextConversionService
{
    static TextConversionService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding GetEncoding(string encodingName)
    {
        try
        {
            if (string.Equals(encodingName, "shift_jis", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(encodingName, "sjis", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.GetEncoding(932);
            }
            return Encoding.GetEncoding(encodingName);
        }
        catch
        {
            return Encoding.UTF8;
        }
    }

    private static readonly HashSet<string> ReservedDeviceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static string SanitizeFileName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return "無題";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();
        foreach (var c in rawName)
        {
            if (invalidChars.Contains(c) || c == '/' || c == '\\')
                sb.Append('_');
            else
                sb.Append(c);
        }

        var sanitized = sb.ToString();

        // 連続ドット (..) の完全無害化
        while (sanitized.Contains(".."))
        {
            sanitized = sanitized.Replace("..", "_");
        }

        sanitized = sanitized.Trim(' ', '.', '_');
        if (string.IsNullOrEmpty(sanitized))
            return "無題";

        // Windows 予約デバイス名 (CON, PRN, AUX, NUL, COM1-9, LPT1-9) の防御
        string nameWithoutExt = sanitized;
        int dotIdx = sanitized.IndexOf('.');
        if (dotIdx > 0)
        {
            nameWithoutExt = sanitized.Substring(0, dotIdx);
        }

        if (ReservedDeviceNames.Contains(nameWithoutExt))
        {
            sanitized = "_" + sanitized;
        }

        // ファイル名長制限 (Windows MAX_PATH考慮: 最大100文字に制限)
        if (sanitized.Length > 100)
        {
            sanitized = sanitized.Substring(0, 100).TrimEnd(' ', '.');
        }

        return string.IsNullOrEmpty(sanitized) ? "無題" : sanitized;
    }

    /// <summary>
    /// 全ページを指定ディレクトリに個別の .txt ファイルとして一括エクスポート
    /// </summary>
    public ExportResult ExportPagesToDirectory(IReadOnlyList<NoteXPage> pages, ExportOptions options)
    {
        var result = new ExportResult();
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            result.Errors.Add("出力先フォルダが指定されていません。");
            return result;
        }

        string fullOutputDir = Path.GetFullPath(options.OutputDirectory);
        if (!Directory.Exists(fullOutputDir))
        {
            Directory.CreateDirectory(fullOutputDir);
        }

        var encoding = GetEncoding(options.EncodingName);
        var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < pages.Count; i++)
        {
            var page = pages[i];
            string safeTitle = SanitizeFileName(page.Title);

            string baseFileName = options.IncludePageNumberPrefix
                ? $"{(i + 1):D2}_{safeTitle}"
                : safeTitle;

            string fileName = $"{baseFileName}.txt";
            int duplicateCounter = 1;
            while (usedFileNames.Contains(fileName))
            {
                fileName = $"{baseFileName}_{duplicateCounter}.txt";
                duplicateCounter++;
            }
            usedFileNames.Add(fileName);

            string targetPath = Path.GetFullPath(Path.Combine(fullOutputDir, fileName));

            // パストラバーサル脆弱性防御: 出力先ディレクトリ外への書き込みを阻止
            string dirWithSeparator = fullOutputDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!targetPath.StartsWith(dirWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add($"セキュリティ警告: 不正なファイルパスへの書き込みがブロックされました: [{fileName}]");
                continue;
            }

            if (File.Exists(targetPath) && !options.OverwriteExisting)
            {
                result.SkippedFiles.Add(targetPath);
                continue;
            }

            try
            {
                string normalizedContent = NormalizeNewLines(page.Content, options.NewLine);
                File.WriteAllText(targetPath, normalizedContent, encoding);
                result.ExportedFiles.Add(targetPath);
                result.ExportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"[{fileName}] の出力に失敗しました: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 単一ページをテキストファイルとして書き出し
    /// </summary>
    public void ExportSinglePage(NoteXPage page, string targetFilePath, string encodingName = "utf-8", string newLine = "\r\n")
    {
        var encoding = GetEncoding(encodingName);
        string normalizedContent = NormalizeNewLines(page.Content, newLine);
        string dir = Path.GetDirectoryName(targetFilePath) ?? string.Empty;
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(targetFilePath, normalizedContent, encoding);
    }

    /// <summary>
    /// 全ページを1つのテキストファイルにまとめてエクスポート
    /// </summary>
    public void ExportMergedDocument(NoteXDocument document, string targetFilePath, string separatorFormat = "===== {0} =====", string encodingName = "utf-8", string newLine = "\r\n")
    {
        var encoding = GetEncoding(encodingName);
        var sb = new StringBuilder();

        for (int i = 0; i < document.Pages.Count; i++)
        {
            var page = document.Pages[i];
            if (i > 0)
            {
                sb.Append(newLine).Append(newLine);
            }
            sb.Append(string.Format(separatorFormat, page.Title)).Append(newLine);
            sb.Append(NormalizeNewLines(page.Content, newLine));
        }

        string dir = Path.GetDirectoryName(targetFilePath) ?? string.Empty;
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(targetFilePath, sb.ToString(), encoding);
    }

    /// <summary>
    /// 複数のテキストファイルからページを読み込んでインポート
    /// </summary>
    public List<NoteXPage> ImportFiles(IEnumerable<string> filePaths)
    {
        var importedPages = new List<NoteXPage>();

        foreach (var path in filePaths)
        {
            if (!File.Exists(path)) continue;

            string title = Path.GetFileNameWithoutExtension(path);
            var (content, encoding) = DetectAndReadAllText(path);

            importedPages.Add(new NoteXPage
            {
                Title = title,
                Content = content,
                Encoding = encoding.WebName,
                CreatedAt = File.GetCreationTime(path),
                ModifiedAt = File.GetLastWriteTime(path)
            });
        }

        return importedPages;
    }

    /// <summary>
    /// ディレクトリ内の全.txtファイルをインポート
    /// </summary>
    public List<NoteXPage> ImportDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return new List<NoteXPage>();

        var files = Directory.GetFiles(directoryPath, "*.txt")
            .OrderBy(f => f)
            .ToList();

        return ImportFiles(files);
    }

    private static string NormalizeNewLines(string text, string targetNewLine)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", targetNewLine);
    }

    public const long MaxImportFileSize = 50 * 1024 * 1024; // 50MB

    /// <summary>
    /// ファイルのエンコーディング（UTF-8, UTF-8 BOM, Shift_JIS）を自動判別してテキスト読込
    /// </summary>
    public static (string Content, Encoding Encoding) DetectAndReadAllText(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > MaxImportFileSize)
        {
            throw new InvalidDataException($"ファイルサイズが大きすぎます ({fileInfo.Length / (1024 * 1024)}MB)。50MB以下のファイルを指定してください。");
        }
        if (fileInfo.Length == 0)
        {
            return (string.Empty, Encoding.UTF8);
        }

        byte[] bytes = File.ReadAllBytes(filePath);

        // BOMチェック
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return (Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3), Encoding.UTF8);
        }
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return (Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2), Encoding.Unicode);
        }

        // UTF-8 チェック
        if (IsValidUtf8(bytes))
        {
            return (Encoding.UTF8.GetString(bytes), Encoding.UTF8);
        }

        // 日本語環境用フォールバック (Shift_JIS)
        try
        {
            var sjis = Encoding.GetEncoding(932);
            return (sjis.GetString(bytes), sjis);
        }
        catch
        {
            return (Encoding.UTF8.GetString(bytes), Encoding.UTF8);
        }
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        int expectedLength = 0;
        foreach (byte b in bytes)
        {
            if (expectedLength == 0)
            {
                if ((b & 0b10000000) == 0) continue; // 1 byte ASCII
                if ((b & 0b11100000) == 0b11000000) expectedLength = 1; // 2 byte
                else if ((b & 0b11110000) == 0b11100000) expectedLength = 2; // 3 byte
                else if ((b & 0b11111000) == 0b11110000) expectedLength = 3; // 4 byte
                else return false;
            }
            else
            {
                if ((b & 0b11000000) != 0b10000000) return false;
                expectedLength--;
            }
        }
        return expectedLength == 0;
    }
}
