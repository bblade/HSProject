using HSProject.Models;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers; 
public class BlacklistController : ControllerBase {

    [HttpPost("api/check")]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {
        IEnumerable<Goods> goods = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;

        var filteredGoods = goods
            .Where(g =>
                blacklistEntries.Any(entry =>
                    g.Title
                        .Split(' ')
                        .Intersect(entry.Text.Split(' '), StringComparer.OrdinalIgnoreCase).Any()
                )
            )
            .Select(g => {
                var matchedBlacklistIds = blacklistEntries
                    .Where(entry => g.Title.Split(' ')
                        .Intersect(entry.Text.Split(' '), StringComparer.OrdinalIgnoreCase).Any())
                    .Select(entry => entry.Id)
                    .ToList();

                return new {
                    g.Id,
                    g.Title,
                    MatchedBlacklistIds = matchedBlacklistIds.Distinct()
                };
            })
            .ToList();

        return Ok(filteredGoods);
    }
}
