using HSProject.Models;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Concurrent;

namespace HSProject.Controllers;
public class BlacklistController : ControllerBase {

    [HttpPost("api/check")]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {
        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;
        ConcurrentBag<OutputEntry> outputList = [];

        Parallel.ForEach(blacklistEntries, blacklistEntry => {
            blacklistEntry.Words = blacklistEntry.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        });

        Parallel.ForEach(goodsList, goods => {
            var blacklistIds = blacklistEntries
                .AsParallel()
                .Where(entry =>
                    goods.Title.Split(' ')
                    .Intersect(entry.Words, StringComparer.OrdinalIgnoreCase).Any())
                    .Select(entry => entry.Id);

            if (blacklistIds.Any()) {
                OutputEntry outputEntry = new() {
                    GoodsId = goods.Id,
                    BlacklistIds = blacklistIds
                };
                outputList.Add(outputEntry);
            }
        });

        List<OutputEntry> list = outputList.ToList();

        OutputDto outputDto = new() {
            Count = list.Count,
            Data = list
            
        };


        return Ok(outputDto);
    }
}
