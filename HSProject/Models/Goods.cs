using System.Text.Json.Serialization;

namespace HSProject.Models;
public class Goods {
    public string HsCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public decimal PriceEur { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Weight { get; set; }

    [JsonIgnore]
    public List<GoodsListWord> Words { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public decimal PriceLimit { get; set; }
    public bool IsBanned { get; set; }
}
