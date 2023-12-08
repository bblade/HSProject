using HSProject.Models;

using System.Collections.Concurrent;

namespace HSProject.Services;
public class BlacklistService {

    public OutputDto Check(BlacklistDto blacklistDto) {
        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;
        ConcurrentBag<OutputEntry> outputList = [];

        Parallel.ForEach(blacklistEntries
            .Where(b =>
                b.ComparisonType.Equals("AnyWord") ||
                b.ComparisonType.Equals("AllWords")),
            blacklistEntry => {
                blacklistEntry.Words = blacklistEntry.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            });

        Parallel.ForEach(goodsList, goods => {
            var blacklistHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("AnyWord"))
                .Where(entry =>
                    goods.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Intersect(entry.Words, StringComparer.OrdinalIgnoreCase).Any()).ToList();

            var blacklistAllWordsHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("AllWords"))
                .Where(entry => entry.Words.All(word =>
                    goods.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Contains(word, StringComparer.OrdinalIgnoreCase))).ToList();

            var blacklistExactHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("Exact"))
                .Where(entry => goods.Title.Contains(entry.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();


            var blacklistTotalHits = blacklistHits.Concat(blacklistAllWordsHits).Concat(blacklistExactHits);

            if (blacklistTotalHits.Any()) {
                OutputEntry outputEntry = new() {
                    Goods = goods,
                    BlacklistEntries = [.. blacklistTotalHits]
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

    //public OutputDto Check2(BlacklistDto blacklistDto) {
    //    IEnumerable<Goods> goodsList = blacklistDto.Goods;
    //    IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;


    //    List<BlacklistOutputEntry> blacklistOutputEntries = [];

    //    Parallel.ForEach(blacklistEntries, blacklistEntry => {
    //        blacklistEntry.Words = blacklistEntry.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //    });

    //    foreach (var blackListEntry in blacklistEntries) {
    //        BlacklistOutputEntry? blacklistOutputEntry = null;
    //        foreach (var word in blackListEntry.Words) {

    //            foreach (var goods in goodsList) {
    //                if (goods.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)) {
    //                    blacklistOutputEntry ??= new() {
    //                        Id = blackListEntry.Id
    //                    };
    //                    blacklistOutputEntry.Goods.Add(goods.Id);
    //                }
    //            }
    //        }

    //        if (blacklistOutputEntry != null) {
    //            blacklistOutputEntries.Add(blacklistOutputEntry);
    //        }
    //    }

    //    IEnumerable<string> goodsIds = blacklistOutputEntries.SelectMany(e => e.Goods).Distinct();
    //    List<OutputEntry> data = [];

    //    foreach (var goods in goodsIds) {
    //        data.Add(new OutputEntry() {
    //            Goods = goods,
    //            BlacklistEntries = blacklistOutputEntries.Where(b => b.Goods.Contains(goods)).Select(b => b.Id).ToList()
    //        });
    //    }

    //    OutputDto outputDto = new() {
    //        Count = data.Count,
    //        Data = data

    //    };

    //    return outputDto;
    //}

    //public class BlacklistOutputEntry {
    //    public string Id { get; set; } = string.Empty;
    //    public List<string> Goods { get; } = [];
    //}
}
