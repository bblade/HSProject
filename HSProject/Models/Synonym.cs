using System.Text.Json.Serialization;

namespace HSProject.Models; 
public class Synonym : ISynonym {
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string ComparisonType { get; set; } = "AllWords";

    [JsonIgnore]
    public IEnumerable<string> Words { get; set; } = [];
}
