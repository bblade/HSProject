using System.Text.Json.Serialization;

namespace HSProject.Api.Models;
public class FileMakerRecordDto(string body, string script) {
    [JsonPropertyName("fieldData")]
    public FileMakerFieldDataDto FieldData { get; init; } = new(body, script);
    [JsonPropertyName("script")]
    public string Script { get; init; } = script;
}

