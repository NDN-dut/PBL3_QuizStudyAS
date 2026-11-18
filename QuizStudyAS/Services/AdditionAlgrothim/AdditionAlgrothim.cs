using System.Globalization;
using System.Text;
using QuizStudyAS.Data;

namespace QuizStudyAS.Services.AdditionAlgorithm
{
    public class AdditionAlgorithm : IAdditionAlgorithm
    {
        private readonly AppDbContext? _context;

        public AdditionAlgorithm(AppDbContext? context = null)
        {
            _context = context;
        }

        public int DistanceLevenshtein(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            int l1 = s1.Length, l2 = s2.Length;
            int[,] dp = new int[l1 + 1, l2 + 1];
            for (int i = 0; i <= l1; i++)
            {
                dp[i, 0] = i;
            }
            for (int i = 0; i <= l2; i++)
            {
                dp[0, i] = i;
            }
            for (int i = 1; i <= l1; i++)
            {
                for (int j = 1; j <= l2; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    int replace = dp[i - 1, j - 1] + cost;
                    int dele = dp[i - 1, j] + 1;
                    int append = dp[i, j - 1] + 1;

                    int nho = Math.Min(replace, dele);
                    nho = Math.Min(nho, append);
                    dp[i, j] = nho;
                }
            }
            return dp[l1, l2];
        }

        // Minimum edit distance between `pattern` and ANY substring of `text`
        // (Sellers' algorithm: row 0 is all zeros so a match may start anywhere in `text`,
        //  and the answer is the minimum of the last row so it may end anywhere).
        // Swapping two adjacent characters (a very common typo, e.g. "netwrok") counts as 1 edit.
        // This lets "jav programing" still match "Advanced Java Programming".
        public int ApproximateSubstringDistance(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return 0;
            if (string.IsNullOrEmpty(text)) return pattern.Length;

            int m = pattern.Length, n = text.Length;
            var prevPrev = new int[n + 1];   // row i-2 (needed for transpositions)
            var prev = new int[n + 1];       // row i-1; row 0 is all zeros
            var cur = new int[n + 1];        // row i

            for (int i = 1; i <= m; i++)
            {
                cur[0] = i;
                for (int j = 1; j <= n; j++)
                {
                    int cost = pattern[i - 1] == text[j - 1] ? 0 : 1;
                    int best = Math.Min(prev[j - 1] + cost, Math.Min(prev[j] + 1, cur[j - 1] + 1));

                    if (i > 1 && j > 1 && pattern[i - 1] == text[j - 2] && pattern[i - 2] == text[j - 1])
                    {
                        best = Math.Min(best, prevPrev[j - 2] + 1);
                    }
                    cur[j] = best;
                }

                // Rotate the three rows instead of allocating a 2D table: O(n) memory
                var recycled = prevPrev;
                prevPrev = prev;
                prev = cur;
                cur = recycled;
            }

            int result = int.MaxValue;
            for (int j = 0; j <= n; j++)
            {
                result = Math.Min(result, prev[j]);
            }
            return result;
        }

        // Normalize text before matching: lower case, strip Vietnamese accents, collapse spaces.
        // Example: "  Lập   Trình Java " -> "lap trinh java"
        public static string NormalizeForSearch(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string decomposed = input.Trim()
                                     .ToLowerInvariant()
                                     .Replace('đ', 'd')
                                     .Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder(decomposed.Length);
            bool lastWasSpace = false;
            foreach (char ch in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                {
                    continue; // drop accent marks
                }
                if (char.IsWhiteSpace(ch))
                {
                    if (!lastWasSpace) sb.Append(' ');
                    lastWasSpace = true;
                    continue;
                }
                sb.Append(ch);
                lastWasSpace = false;
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // How many typos are tolerated for a (normalized) keyword of the given length.
        // Short keywords must match exactly, otherwise "xyz" would return random classes.
        public static int MaxTyposFor(int keywordLength)
        {
            if (keywordLength <= 3) return 0;
            if (keywordLength <= 7) return 1;
            return Math.Min(3, keywordLength / 4);
        }
    }
}
