namespace HSProject.Models; 
public class OutputEntry {
    public string GoodsId { get; set; } = string.Empty;
    public IEnumerable<string> BlacklistIds { get; set; } = [];

}
