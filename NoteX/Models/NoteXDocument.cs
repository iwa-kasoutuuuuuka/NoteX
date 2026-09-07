using System.Text.Json.Serialization;

namespace NoteX.Models;

public class NoteXDocument
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = "NoteX";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "無題";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("activePageIndex")]
    public int ActivePageIndex { get; set; } = 0;

    [JsonPropertyName("pages")]
    public List<NoteXPage> Pages { get; set; } = new();

    public static NoteXDocument CreateDefault(string title = "無題")
    {
        var doc = new NoteXDocument
        {
            Title = title,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ActivePageIndex = 0,
            Pages = new List<NoteXPage>
            {
                new NoteXPage
                {
                    Title = "ページ 1",
                    Content = string.Empty,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                }
            }
        };
        return doc;
    }
}
