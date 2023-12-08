namespace HSProject.Models; 
public class OutputEntry {
    public Goods? Goods { get; set; }
    public IEnumerable<BlacklistEntry>? BlacklistEntries { get; set; } = [];

}
