//using System.Text.Json.Serialization;

namespace HSProject.Models; 
public class Synonym  {
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string ComparisonType { get; set; } = "AllWords";

    //[JsonIgnore]
    public IEnumerable<SynonymWord> Words { get; set; } = [];
}
