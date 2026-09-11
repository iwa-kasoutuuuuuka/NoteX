using System.IO;
using System.Text;
using NoteX.Models;
using NoteX.Services;
using Xunit;

namespace NoteX.Tests;

public class FileAndConversionTests : IDisposable
{
    private readonly string _testDir;

    public FileAndConversionTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "NoteXTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    [Fact]
    public void SaveAndLoad_TxtxDocument_PreservesAllData()
    {
        // Arrange
        var fileService = new FileService();
        var doc = NoteXDocument.CreateDefault("テストノート");
        doc.Pages.Add(new NoteXPage
        {
            Title = "2ページ目（アイディア）",
            Content = "こんにちは世界！\n改行テスト。"
        });
        doc.ActivePageIndex = 1;

        string filePath = Path.Combine(_testDir, "test.txtx");

        // Act
        fileService.SaveDocument(doc, filePath);
        var loaded = fileService.LoadDocument(filePath);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("テストノート", loaded.Title);
        Assert.Equal(2, loaded.Pages.Count);
        Assert.Equal("ページ 1", loaded.Pages[0].Title);
        Assert.Equal("2ページ目（アイディア）", loaded.Pages[1].Title);
        Assert.Equal("こんにちは世界！\n改行テスト。", loaded.Pages[1].Content);
    }

    [Fact]
    public void ExportPagesToDirectory_CreatesIndividualTxtFilesWithPrefix()
    {
        // Arrange
        var conversionService = new TextConversionService();
        var pages = new List<NoteXPage>
        {
            new NoteXPage { Title = "第1章 はじめに", Content = "第1章のテキストです。" },
            new NoteXPage { Title = "第2章 本論", Content = "第2章の内容です。" }
        };

        var options = new ExportOptions
        {
            OutputDirectory = Path.Combine(_testDir, "ExportTxt"),
            IncludePageNumberPrefix = true,
            EncodingName = "utf-8",
            NewLine = "\r\n"
        };

        // Act
        var result = conversionService.ExportPagesToDirectory(pages, options);

        // Assert
        Assert.Equal(2, result.ExportedCount);
        Assert.Empty(result.Errors);

        string file1 = Path.Combine(options.OutputDirectory, "01_第1章 はじめに.txt");
        string file2 = Path.Combine(options.OutputDirectory, "02_第2章 本論.txt");

        Assert.True(File.Exists(file1));
        Assert.True(File.Exists(file2));
        Assert.Equal("第1章のテキストです。", File.ReadAllText(file1));
        Assert.Equal("第2章の内容です。", File.ReadAllText(file2));
    }

    [Fact]
    public void ExportMergedDocument_CombinesPagesWithSeparator()
    {
        // Arrange
        var conversionService = new TextConversionService();
        var doc = new NoteXDocument
        {
            Pages = new List<NoteXPage>
            {
                new NoteXPage { Title = "セクションA", Content = "コンテンツA" },
                new NoteXPage { Title = "セクションB", Content = "コンテンツB" }
            }
        };
        string targetFile = Path.Combine(_testDir, "merged.txt");

        // Act
        conversionService.ExportMergedDocument(doc, targetFile);

        // Assert
        Assert.True(File.Exists(targetFile));
        string content = File.ReadAllText(targetFile);
        Assert.Contains("===== セクションA =====", content);
        Assert.Contains("コンテンツA", content);
        Assert.Contains("===== セクションB =====", content);
        Assert.Contains("コンテンツB", content);
    }

    [Fact]
    public void ImportFiles_CreatesPagesWithTitlesFromFileName()
    {
        // Arrange
        var conversionService = new TextConversionService();
        string fileA = Path.Combine(_testDir, "買い物リスト.txt");
        string fileB = Path.Combine(_testDir, "メモ_2026.txt");

        File.WriteAllText(fileA, "りんご\nみかん", Encoding.UTF8);
        File.WriteAllText(fileB, "重要な会議メモ", Encoding.UTF8);

        // Act
        var pages = conversionService.ImportFiles(new[] { fileA, fileB });

        // Assert
        Assert.Equal(2, pages.Count);
        Assert.Equal("買い物リスト", pages[0].Title);
        Assert.Equal("りんご\nみかん", pages[0].Content);
        Assert.Equal("メモ_2026", pages[1].Title);
        Assert.Equal("重要な会議メモ", pages[1].Content);
    }

    [Fact]
    public void ImportFiles_DecodesUtf16BigEndianProperly()
    {
        // Arrange
        var conversionService = new TextConversionService();
        string fileBe = Path.Combine(_testDir, "utf16be.txt");
        // UTF-16 Big Endian with BOM (0xFE, 0xFF)
        File.WriteAllText(fileBe, "こんにちはUTF16BE", Encoding.BigEndianUnicode);

        // Act
        var pages = conversionService.ImportFiles(new[] { fileBe });

        // Assert
        Assert.Single(pages);
        Assert.Equal("こんにちはUTF16BE", pages[0].Content);
    }

    [Fact]
    public void ImportFiles_SkipsLockedFileGracefully()
    {
        // Arrange
        var conversionService = new TextConversionService();
        string lockedFile = Path.Combine(_testDir, "locked.txt");
        string normalFile = Path.Combine(_testDir, "normal.txt");
        File.WriteAllText(lockedFile, "ロックファイル");
        File.WriteAllText(normalFile, "通常ファイル");

        // Lock the file exclusively
        using (var fs = new FileStream(lockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // Act
            var pages = conversionService.ImportFiles(new[] { lockedFile, normalFile });

            // Assert: ロックされたファイルは安全にスキップされ、正常なファイルのみインポートされること
            Assert.Single(pages);
            Assert.Equal("通常ファイル", pages[0].Content);
        }
    }
}
