using HSProject.Models;

using System.Collections.Concurrent;

namespace HSProject.Services;
public class BlacklistService {

    private static double allowedDifference;
    private static readonly char[] separatorChars = [' ', ',', ';', '[', ']', '{', '}', '(', ')'];

    public OutputDto Check(InputDto inputDto) {

        allowedDifference = inputDto.AllowedDifference;

        IEnumerable<Goods> goodsList = inputDto.Goods;
        IEnumerable<BlacklistEntry> blacklistEntries = inputDto.Blacklist;
        ConcurrentBag<OutputEntry> outputList = [];
        IEnumerable<string> exceptWords = inputDto.ExceptWords;
        IEnumerable<HsCode> hsCodes = inputDto.HsCodes;
        decimal defaultPriceLimit = inputDto.DefaultPriceLimit;
        IEnumerable<string> whitelistTags = inputDto.WhitelistTags;

        Parallel.ForEach(blacklistEntries
            .Where(b =>
                b.ComparisonType.Equals("AnyWord") ||
                b.ComparisonType.Equals("AllWords")),
            blacklistEntry => {
                blacklistEntry.Words = blacklistEntry.Text
                    .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                    .Except(exceptWords, StringComparer.InvariantCultureIgnoreCase);
            });

        Parallel.ForEach(goodsList, goods => {

            var hsCode = hsCodes.FirstOrDefault(hs => hs.Code.Equals(goods.HsCode));

            if (hsCode != null) {

                goods.Status = "hsCode";

                if (hsCode.IsBlacklisted) {
                    goods.IsBanned = true;

                    OutputEntry outputEntry = new() {
                        Goods = goods
                    };
                    outputList.Add(outputEntry);

                } else if (goods.PriceEur > hsCode.PriceLimit) {
                    goods.PriceLimit = hsCode.PriceLimit;

                    OutputEntry outputEntry = new() {
                        Goods = goods
                    };
                    outputList.Add(outputEntry);
                }

            } else {

                var goodsTitleWords = goods.Title
                    .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                    .Except(exceptWords, StringComparer.InvariantCultureIgnoreCase)
                    .Except(whitelistTags, StringComparer.InvariantCultureIgnoreCase)
                    .Select(w => new GoodsListWord() {
                        Word = w
                    });

                goods.Words = [.. goodsTitleWords];


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
                    goods.Words.Add(new() {
                        Precision = 1,
                        Word = blacklistEntry.Text
                    });
                }

                IEnumerable<BlacklistEntry> blacklistTotalHits = blacklistAnyWordHits
                    .Concat(blacklistAllWordsHits)
                    .Concat(blacklistExactHits);

                if (blacklistTotalHits.Any()) {
                    goods.Status = "stopList";
                    OutputEntry outputEntry = new() {
                        Goods = goods,
                        BlacklistEntries = [.. blacklistTotalHits]
                    };
                    outputList.Add(outputEntry);
                } else if (goods.PriceEur > defaultPriceLimit) {
                    goods.Status = "priceLimit";
                    OutputEntry outputEntry = new() {
                        Goods = goods
                    };
                    outputList.Add(outputEntry);
                }
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

        foreach (GoodsListWord word in goods.Words) {
            foreach (string blacklistWord in blacklistEntry.Words) {

                double difference = Comparer
                    .CalculateLevenshteinDistance(word.Word, blacklistWord);

                if (difference <= allowedDifference) {
                    double precision = 1 - difference;
                    if (word.Precision < precision) {
                        word.Precision = precision;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    static bool IsAllWordsBlacklisted(Goods goods, BlacklistEntry blacklistEntry) {

        foreach (string blacklistWord in blacklistEntry.Words) {
            bool matchFound = false;

            foreach (GoodsListWord word in goods.Words) {
                double difference = Comparer
                    .CalculateLevenshteinDistance(word.Word, blacklistWord);

                if (difference <= allowedDifference) {
                    double precision = 1 - difference;
                    if (word.Precision < precision) {
                        word.Precision = precision;
                    }
                    matchFound = true;
                }
            }

            if (!matchFound) {
                return false;
            }
        }

        return true;
    }
}