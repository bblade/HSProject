using HSProject.Models;

using System.Collections.Concurrent;

namespace HSProject.Services;
public class BlacklistService {

    private static double allowedDifference;
    private static readonly char[] separatorChars = [' ', ',', ';', '[', ']', '{', '}'];
    private static readonly object lockObject = new();

    public OutputDto Check(BlacklistDto blacklistDto) {

        allowedDifference = blacklistDto.AllowedDifference;

        IEnumerable<Goods> goodsList = blacklistDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = blacklistDto.Blacklist;
        ConcurrentBag<OutputEntry> outputList = [];
        IEnumerable<string> exceptWords = blacklistDto.ExceptWords;

        // split blacklist entries into words
        Parallel.ForEach(blacklistEntries
            .Where(b =>
                b.ComparisonType.Equals("AnyWord") ||
                b.ComparisonType.Equals("AllWords")),
            blacklistEntry => {
                blacklistEntry.Words = blacklistEntry.Text
                    .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                    .Except(exceptWords);
            });

        // split goods title entries into words
        Parallel.ForEach(goodsList, goods => {
            var goodsTitleWords = goods.Title
                .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                .Except(exceptWords);

            goods.Words = [.. goodsTitleWords];

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
                .Where(entry => goods.Title
                    .Contains(entry.Text, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

            foreach (var blacklistEntry in blacklistExactHits) {
                goods.WordHits.Add(new() { 
                    Precision = 1, 
                    Word = blacklistEntry.Text 
                });
            }

            var blacklistTotalHits = blacklistAnyWordHits
                .Concat(blacklistAllWordsHits)
                .Concat(blacklistExactHits);

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
        };

        foreach (var item in list) {
            outputDto.Data.Add(item.Goods?.Id ?? string.Empty, item);
        }

        return outputDto;
    }

    static bool IsAnyWordBlacklisted(Goods goods, BlacklistEntry blacklistEntry) {

        foreach (string word in goods.Words) {
            foreach (string blacklistWord in blacklistEntry.Words) {

                double difference = Comparer
                    .CalculateLevenshteinDistance(word, blacklistWord);

                if (difference <= allowedDifference) {
                    lock (lockObject) {
                        GoodsListWord? goodsListWord = goods.WordHits
                            .FirstOrDefault(w => w.Word == word);
                        if (goodsListWord == null) {
                            goodsListWord = new GoodsListWord() { Word = word };
                            goods.WordHits.Add(goodsListWord);
                        }
                        double precision = 1 - difference;
                        if (goodsListWord.Precision < precision) {
                            goodsListWord.Precision = precision;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    static bool IsAllWordsBlacklisted(Goods goods, BlacklistEntry blacklistEntry) {

        foreach (string blacklistWord in blacklistEntry.Words) {
            bool matchFound = false;

            foreach (string word in goods.Words) {
                double difference = Comparer
                    .CalculateLevenshteinDistance(word, blacklistWord);

                if (difference <= allowedDifference) {
                    lock (lockObject) {
                        GoodsListWord? goodsListWord = goods.WordHits
                            .FirstOrDefault(w => w.Word == word);
                        if (goodsListWord == null) {
                            goodsListWord = new GoodsListWord() { Word = word };
                            goods.WordHits.Add(goodsListWord);
                        }
                        double precision = 1 - difference;
                        if (goodsListWord.Precision < precision) {
                            goodsListWord.Precision = precision;
                        }

                        matchFound = true;
                    }
                }
            }

            if (!matchFound) {
                return false;
            }
        }

        return true;
    }
}