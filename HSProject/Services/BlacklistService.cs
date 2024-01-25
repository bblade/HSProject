using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;

using HSProject.Models;

using System.Collections.Concurrent;
using System.Linq;

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
                    .Where(b => IsAnyWordHit(goods, b)).ToList();

                var blacklistAllWordsHits = blacklistEntries
                    .AsParallel()
                    .Where(entry => entry.ComparisonType.Equals("AllWords"))
                    .Where(b => IsAllWordsHit(goods, b)).ToList();

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

    public IEnumerable<Goods> CheckV2(InputDto inputDto) {

        allowedDifference = inputDto.AllowedDifference;

        IEnumerable<Goods> goodsList = inputDto.Goods;
        IEnumerable<string> exceptWords = inputDto.ExceptWords;
        IEnumerable<HsCode> hsCodes = inputDto.HsCodes;
        decimal defaultPriceLimit = inputDto.DefaultPriceLimit;
        IEnumerable<string> whitelistTags = inputDto.WhitelistTags;

        Parallel.ForEach(hsCodes, hsCode =>
            Parallel.ForEach(hsCode.Synonyms
                .Where(b =>
                    b.ComparisonType.Equals("AnyWord") ||
                    b.ComparisonType.Equals("AllWords")),
                synonym => {
                    synonym.Words = synonym.Label
                        .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                        .Except(exceptWords, StringComparer.InvariantCultureIgnoreCase)
                        .Select(word => new SynonymWord { Text = word })
                        .ToList();
                })
        );

        foreach (Goods goods in goodsList) {

            foreach (var hsCode in hsCodes.Where(hs => hs.DirectMatch.Count != 0)) {
                foreach (var directMatch in hsCode.DirectMatch) {
                    if (goods.Title == "Socks") {
                        Console.WriteLine("Socks");
                    }
                    if (directMatch.Label.Equals(goods.Title, StringComparison.InvariantCultureIgnoreCase)) {
                        goods.Matches.Add(new Match() {
                            HsCode = hsCode.Code,
                            DirectMatch = directMatch.Label
                        });

                        if (string.IsNullOrWhiteSpace(goods.AssignedHsCode)) {
                            goods.AssignedHsCode = hsCode.Code;
                        }
                    }
                }
            }

            var goodsTitleWords = goods.Title
                .Split(separatorChars, StringSplitOptions.RemoveEmptyEntries)
                .Except(exceptWords, StringComparer.InvariantCultureIgnoreCase)
                .Except(whitelistTags, StringComparer.InvariantCultureIgnoreCase)
                .Select(w => new GoodsListWord() {
                    Word = w
                });

            goods.Words = [.. goodsTitleWords];

            foreach (HsCode hsCode in hsCodes) {
                var synonymAnyWordHits = hsCode.Synonyms
                    .AsParallel()
                    .Where(entry => entry.ComparisonType.Equals("AnyWord"))
                    .Where(b => IsAnyWordHitV2(goods, b)).ToList();

                var synonymAllWordHits = hsCode.Synonyms
                    .AsParallel()
                    .Where(entry => entry.ComparisonType.Equals("AllWords"))
                    .Where(b => IsAllWordsHitV2(goods, b)).ToList();

                var hits = synonymAllWordHits.Union(synonymAnyWordHits);

                foreach (var hit in hits) {
                    goods.Matches.Add(new Match() {
                        HsCode = hsCode.Code,
                        Synonym = hit
                    });
                }
            }

            if (string.IsNullOrEmpty(goods.AssignedHsCode) && goods.Matches.Count != 0) {
                goods.AssignedHsCode = goods.Matches
                    .OrderByDescending(m =>
                        m.Synonym.Words
                        .Average(w => w.Accuracy))
                    .FirstOrDefault().HsCode ?? "";
                continue;
            }
        }

        return goodsList;
    }

    static bool IsAnyWordHit(Goods goods, ISynonym synonym) {

        foreach (GoodsListWord word in goods.Words) {
            foreach (string synonymWord in synonym.Words) {

                double difference = Comparer
                    .CalculateLevenshteinDistance(word.Word, synonymWord);

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

    static bool IsAllWordsHit(Goods goods, ISynonym synonym) {

        foreach (string blacklistWord in synonym.Words) {
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

    static bool IsAnyWordHitV2(Goods goods, Synonym synonym) {

        foreach (GoodsListWord word in goods.Words) {
            foreach (SynonymWord synonymWord in synonym.Words) {

                double difference = Comparer
                    .CalculateLevenshteinDistance(word.Word, synonymWord.Text);

                if (difference <= allowedDifference) {
                    double accuracy = 1 - difference;
                    synonymWord.Accuracy = accuracy;
                    return true;
                }
            }
        }
        return false;
    }

    static bool IsAllWordsHitV2(Goods goods, Synonym synonym) {

        foreach (SynonymWord synonymWord in synonym.Words) {
            bool matchFound = false;

            foreach (GoodsListWord word in goods.Words) {
                double difference = Comparer
                    .CalculateLevenshteinDistance(word.Word, synonymWord.Text);

                if (difference <= allowedDifference) {
                    double accuracy = 1 - difference;
                    synonymWord.Accuracy = accuracy;
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