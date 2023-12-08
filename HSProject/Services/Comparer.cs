namespace HSProject.Services;
public static class Comparer {
    public static List<string> FindMatchingWords(List<string> list1, List<string> list2, int maxDifference) {
        List<string> matchingWords = [];

        foreach (string word1 in list1) {
            foreach (string word2 in list2) {
                if (CalculateLevenshteinDistance(word1, word2) <= maxDifference) {
                    matchingWords.Add(word1);
                    break;
                }
            }
        }

        return matchingWords;
    }

    static int CalculateLevenshteinDistance(string s1, string s2) {
        int[,] distances = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++) {
            for (int j = 0; j <= s2.Length; j++) {
                if (i == 0)
                    distances[i, j] = j;
                else if (j == 0)
                    distances[i, j] = i;
                else
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1)
                    );
            }
        }

        return distances[s1.Length, s2.Length];
    }
}
