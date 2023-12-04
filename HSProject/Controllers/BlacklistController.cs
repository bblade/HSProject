using HSProject.Models;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers; 
public class BlacklistController : ControllerBase {

    [HttpPost("api/check")]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {
        IEnumerable<Goods> goods = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;

        var filteredGoods = goods.Select(good => {
            var matchedBlacklistIds = blacklistEntries
                .Where(entry => good.Title.Split(' ')
                    .Intersect(entry.Text.Split(' '), StringComparer.OrdinalIgnoreCase).Any())
                .Select(entry => entry.Id)
                .ToList();

            return new {
                good.Id,
                good.Title,
                MatchedBlacklistIds = matchedBlacklistIds.Distinct()
            };
        }).Where(good => good.MatchedBlacklistIds.Any()) // Filter out goods with no matched blacklist entries
        .ToList();

        return Ok(filteredGoods);
    }
}
