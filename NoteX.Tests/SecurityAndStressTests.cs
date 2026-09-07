using System.IO;
using System.Text;
using NoteX.Models;
using NoteX.Services;
using NoteX.ViewModels;
using Xunit;

namespace NoteX.Tests;

public class SecurityAndStressTests : IDisposable
{
    private readonly string _testDir;

    public SecurityAndStressTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "NoteX_SecTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    [Theory]
    [InlineData("../../evil", "evil")]
    [InlineData("..\\..\\evil", "evil")]
    [InlineData("..", "無題")]
    [InlineData(".", "無題")]
    [InlineData("folder/sub/file", "folder_sub_file")]
    [InlineData("folder\\sub\\file", "folder_sub_file")]
    [InlineData("test:stream", "test_stream")]
    public void SanitizeFileName_PreventsPathTraversal(string input, string expectedSubstring)
    {
        string sanitized = TextConversionService.SanitizeFileName(input);
        Assert.DoesNotContain("/", sanitized);
        Assert.DoesNotContain("\\", sanitized);
        Assert.DoesNotContain("..", sanitized);
        Assert.Contains(expectedSubstring, sanitized);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT1")]
    public void SanitizeFileName_NeutralizesWindowsReservedNames(string reserved)
    {
        string sanitized = TextConversionService.SanitizeFileName(reserved);
        Assert.StartsWith("_", sanitized);
    }

    [Fact]
    public void ExportPagesToDirectory_BlocksPathTraversalAttempts()
    {
        var service = new TextConversionService();
        string exportDir = Path.Combine(_testDir, "TargetExport");
        Directory.CreateDirectory(exportDir);

        var pages = new List<NoteXPage>
        {
            new NoteXPage { Title = "../../../escaped", Content = "traversal content" },
            new NoteXPage { Title = "..\\..\\escaped2", Content = "traversal content 2" }
        };

        var options = new ExportOptions
        {
            OutputDirectory = exportDir,
            IncludePageNumberPrefix = false
        };

        var result = service.ExportPagesToDirectory(pages, options);

        // 出力されたファイルがすべて exportDir の配下にあること
        foreach (var file in result.ExportedFiles)
        {
            string fullPath = Path.GetFullPath(file);
            Assert.StartsWith(Path.GetFullPath(exportDir), fullPath, StringComparison.OrdinalIgnoreCase);
        }

        // 親ディレクトリにファイルが漏洩していないこと
        Assert.False(File.Exists(Path.Combine(_testDir, "escaped.txt")));
        Assert.False(File.Exists(Path.Combine(_testDir, "escaped2.txt")));
    }

    [Fact]
    public void FileService_HandlesEmptyFileGracefully()
    {
        var service = new FileService();
        string emptyFile = Path.Combine(_testDir, "empty.txtx");
        File.WriteAllText(emptyFile, string.Empty);

        var doc = service.LoadDocument(emptyFile);

        Assert.NotNull(doc);
        Assert.NotEmpty(doc.Pages);
        Assert.Equal("empty", doc.Title);
    }

    [Fact]
    public void FileService_RejectsOversizedFiles()
    {
        var service = new FileService();
        string dummyFile = Path.Combine(_testDir, "oversized.txtx");

        // 51MB のダミーストリームを作成
        using (var fs = new FileStream(dummyFile, FileMode.Create, FileAccess.Write))
        {
            fs.SetLength(51L * 1024 * 1024);
        }

        Assert.Throws<InvalidDataException>(() => service.LoadDocument(dummyFile));
    }

    [Fact]
    public void FileService_SanitizesCorruptedPages()
    {
        var service = new FileService();
        string corruptedFile = Path.Combine(_testDir, "corrupted.txtx");

        // titleやcontentがnullのJSON
        string json = "{\"format\":\"NoteX\",\"pages\":[{\"title\":null,\"content\":null}]}";
        File.WriteAllText(corruptedFile, json, Encoding.UTF8);

        var doc = service.LoadDocument(corruptedFile);

        Assert.NotNull(doc);
        Assert.Single(doc.Pages);
        Assert.NotNull(doc.Pages[0].Title);
        Assert.NotNull(doc.Pages[0].Content);
    }

    [Fact]
    public void StressTest_LargeContentHandling()
    {
        var service = new FileService();
        var convService = new TextConversionService();

        // 5,000行、約100,000文字のドキュメントを生成
        var sb = new StringBuilder();
        for (int i = 1; i <= 5000; i++)
        {
            sb.AppendLine($"Line {i:D5}: 日本語のテスト文章です。ABCDEFG 1234567890");
        }

        var doc = NoteXDocument.CreateDefault("大規模ドキュメント");
        doc.Pages[0].Content = sb.ToString();

        string targetTxtx = Path.Combine(_testDir, "stress.txtx");

        // 1. 保存
        service.SaveDocument(doc, targetTxtx);
        Assert.True(File.Exists(targetTxtx));

        // 2. 読込
        var loaded = service.LoadDocument(targetTxtx);
        Assert.Equal(doc.Pages[0].Content, loaded.Pages[0].Content);

        // 3. 一括エクスポート
        string exportDir = Path.Combine(_testDir, "StressExport");
        var result = convService.ExportPagesToDirectory(loaded.Pages, new ExportOptions
        {
            OutputDirectory = exportDir,
            IncludePageNumberPrefix = true
        });

        Assert.Equal(1, result.ExportedCount);
        Assert.True(File.Exists(result.ExportedFiles[0]));
    }
}
