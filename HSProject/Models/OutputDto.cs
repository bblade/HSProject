namespace HSProject.Models;
public class OutputDto  {
    public int Count { get; set; }
    public Dictionary<string, OutputEntry> Data { get; set; } = [];
}
