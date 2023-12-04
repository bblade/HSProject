namespace HSProject.Models;
public class OutputDto {
    public int Count { get; set; }
    public IEnumerable<OutputEntry> Data { get; set; } = [];
}
