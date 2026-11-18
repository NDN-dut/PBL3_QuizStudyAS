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
    }
}
