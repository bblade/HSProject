﻿namespace HSProject.Models; 
public class BlacklistDto {
    const double defaultAllowedDifference = 0.25;
    public double AllowedDifference { get; set; } = defaultAllowedDifference;
    public List<Goods> Goods { get; set; } = [];
    public List<BlacklistEntry> Blacklist { get; set; } = [];
}
