namespace HSProject.Models; 
public class BlacklistDto {
    public List<Goods> Goods { get; set; } = [];
    public List<BlacklistEntry> Blacklist { get; set; } = [];
}
