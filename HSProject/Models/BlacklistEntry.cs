using System.Text.Json.Serialization;

namespace HSProject.Models; 
public class BlacklistEntry : ISynonym {
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    [JsonIgnore]
    public IEnumerable<string> Words { get; set; } = [];

    public string ComparisonType { get; set; } = "AnyWord";
}
