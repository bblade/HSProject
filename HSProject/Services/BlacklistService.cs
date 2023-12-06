using HSProject.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HSProject.Services;
public class BlacklistService {

    public OutputDto Check(BlacklistDto blacklistDto) {
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
                    goods.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
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

        return outputDto;
    }

    public OutputDto Check2(BlacklistDto blacklistDto) {
        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;


        List<BlacklistOutputEntry> blacklistOutputEntries = [];

        Parallel.ForEach(blacklistEntries, blacklistEntry => {
            blacklistEntry.Words = blacklistEntry.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        });

        foreach (var blackListEntry in blacklistEntries) {
            BlacklistOutputEntry? blacklistOutputEntry = null;
            foreach (var word in blackListEntry.Words) {

                foreach (var goods in goodsList) {
                    if (goods.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)) {
                        blacklistOutputEntry ??= new() {
                            Id = blackListEntry.Id
                        };
                        blacklistOutputEntry.Goods.Add(goods.Id);
                    }
                }
            }

            if (blacklistOutputEntry != null) {
                blacklistOutputEntries.Add(blacklistOutputEntry);
            }
        }

        IEnumerable<string> goodsIds = blacklistOutputEntries.SelectMany(e => e.Goods).Distinct();
        List<OutputEntry> data = [];

        foreach (var goods in goodsIds) {
            data.Add(new OutputEntry() {
                GoodsId = goods,
                BlacklistIds = blacklistOutputEntries.Where(b => b.Goods.Contains(goods)).Select(b => b.Id).ToList()
            });
        }

        OutputDto outputDto = new() {
            Count = data.Count,
            Data = data

        };

        return outputDto;
    }

    public class BlacklistOutputEntry {
        public string Id { get; set; } = string.Empty;
        public List<string> Goods { get; } = [];
    }
}
