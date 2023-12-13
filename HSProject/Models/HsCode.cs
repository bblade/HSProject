namespace HSProject.Models; 
public class HsCode {
    public string Code { get; set; } = string.Empty;
    public decimal PriceLimit { get; set; }
    public bool IsBlacklisted { get; set; }
}
