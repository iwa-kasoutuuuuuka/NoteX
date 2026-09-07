using System.Text.Json.Serialization;

namespace NoteX.Models;

public class NoteXPage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = "新規ページ";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = "utf-8";

    public NoteXPage Clone()
    {
        return new NoteXPage
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"{Title} (コピー)",
            Content = Content,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            Encoding = Encoding
        };
    }
}
