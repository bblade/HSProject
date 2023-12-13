namespace HSProject.Models;
public class InputDto {

    const double defaultAllowedDifference = 0.25;
    public double AllowedDifference { get; set; } = defaultAllowedDifference;
    public List<Goods> Goods { get; set; } = [];
    public List<BlacklistEntry> Blacklist { get; set; } = [];
    public IEnumerable<string> ExceptWords { get; set; } = [];
    public IEnumerable<HsCode> HsCodes { get; set; } = [];

}
