using System.Text.Json.Serialization;

namespace HSProject.Models; 
public class GoodsSimple {
    public string HsCode { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [JsonIgnore]
    public List<GoodsListWord> Words { get; set; } = [];
    [JsonIgnore]
    public List<Match> Matches { get; set; } = [];
}
