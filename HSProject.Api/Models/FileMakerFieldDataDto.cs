using System.Text.Json.Serialization;

namespace HSProject.Api.Models; 
public class FileMakerFieldDataDto(string body, string script) {
    [JsonPropertyName("body")]
    public string Body { get; init; } = body;
    [JsonPropertyName("script")]
    public string Script { get; init; } = script;
}
