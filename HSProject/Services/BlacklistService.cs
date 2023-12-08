using HSProject.Models;

using System.Collections.Concurrent;

namespace HSProject.Services;
public class BlacklistService {

    const double allowedDifference = 0.25;
    private static readonly char[] separatorChars = [' ', ',', ';', '[', ']', '{', '}'];

    public OutputDto Check(BlacklistDto blacklistDto) {
        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;
        ConcurrentBag<OutputEntry> outputList = [];

        Parallel.ForEach(blacklistEntries
            .Where(b =>
                b.ComparisonType.Equals("AnyWord") ||
                b.ComparisonType.Equals("AllWords")),
            blacklistEntry => {
                blacklistEntry.Words = blacklistEntry.Text.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries);
            });

        Parallel.ForEach(goodsList, goods => {
            var goodsTitleWords = goods.Title.Split(separatorChars, StringSplitOptions.RemoveEmptyEntries);

            foreach (var goodsTitleWord in goodsTitleWords) {
                var goodsWord = new GoodsListWord {
                    Word = goodsTitleWord
                };
                goods.Words.Add(goodsWord);
            };
        });

        Parallel.ForEach(goodsList, goods => {

            var blacklistAnyWordHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("AnyWord"))
                .Where(b => IsAnyWordBlacklisted(goods, b)).ToList();

            var blacklistAllWordsHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("AllWords"))
                .Where(b => IsAllWordsBlacklisted(goods, b)).ToList();

            var blacklistExactHits = blacklistEntries
                .AsParallel()
                .Where(entry => entry.ComparisonType.Equals("Exact"))
                .Where(entry => goods.Title.Contains(entry.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();


            var blacklistTotalHits = blacklistAnyWordHits.Concat(blacklistAllWordsHits).Concat(blacklistExactHits);

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

    static bool IsAnyWordBlacklisted(Goods goods, BlacklistEntry blacklistEntry) {

        foreach (var word in goods.Words) {
            foreach (var blacklistWord in blacklistEntry.Words) {
                var difference = Comparer.CalculateLevenshteinDistance(word.Word, blacklistWord);
                if (difference == 0) {
                    word.IsExact = true;
                    return true;
                } else if (difference <= allowedDifference) {
                    return true;
                }
            }
        }

        return false;
    }

    static bool IsAllWordsBlacklisted(Goods goods, BlacklistEntry blacklistEntry) {
        foreach (var blacklistWord in blacklistEntry.Words) {
            bool exactMatchFound = false;

            foreach (var word in goods.Words) {
                var distance = Comparer.CalculateLevenshteinDistance(word.Word, blacklistWord);

                if (distance == 0) {
                    word.IsExact = true;
                    exactMatchFound = true;
                    break;
                } else if (distance <= allowedDifference) {
                    exactMatchFound = true;
                    break;
                }
            }

            if (!exactMatchFound) {
                return false;
            }
        }

        return true;
    }



}
