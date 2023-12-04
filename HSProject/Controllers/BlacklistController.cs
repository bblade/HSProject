using HSProject.Models;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;
public class BlacklistController : ControllerBase {

    [HttpPost("api/check")]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {
        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;
        List<OutputEntry> outputList = [];

        foreach (var goods in goodsList) {
            var blacklistIds = blacklistEntries
                .Where(entry =>
                    goods.Title.Split(' ')
                    .Intersect(entry.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase).Any())
                    .Select(entry => entry.Id);

            if (blacklistIds.Any()) {
                OutputEntry outputEntry = new() {
                    GoodsId = goods.Id,
                    BlacklistIds = blacklistIds
                };
                outputList.Add(outputEntry);
            }

        }

        return Ok(outputList);
    }
}
