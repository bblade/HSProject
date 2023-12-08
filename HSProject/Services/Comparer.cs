namespace HSProject.Services;
public static class Comparer {

    public static double CalculateLevenshteinDistance(string s1, string s2) {

        s1 = s1.ToLower();
        s2 = s2.ToLower();
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

        return (double)distances[s1.Length, s2.Length] / s1.Length;
    }
}
