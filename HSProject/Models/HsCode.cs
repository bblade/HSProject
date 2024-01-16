namespace HSProject.Models; 
public class HsCode {
    public string Code { get; set; } = string.Empty;
    public decimal PriceLimit { get; set; }
    public bool IsBlacklisted { get; set; }
    public List<Synonym> DirectMatch { get; set; } = [];
    public List<Synonym> Synonyms { get; set; } = [];
    public Guid Uuid { get; set; }
}
