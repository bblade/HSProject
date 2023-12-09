﻿using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace HSProject.Models; 
public class Goods {
    public string Barcode { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public decimal PriceEur { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Weight { get; set; }

    [JsonIgnore]
    public List<string> Words { get; set; } = [];

    public List<GoodsListWord> WordHits { get; set; } = [];
}
