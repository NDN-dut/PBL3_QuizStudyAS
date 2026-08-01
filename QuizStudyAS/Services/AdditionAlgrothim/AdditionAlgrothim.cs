using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services.AdditionAlgrothim
{
    public class AdditionAlgrothim : IAdditionAlgrothim
    {
        private readonly AppDbContext _context;
        public AdditionAlgrothim(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> CreateUniqueLink()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string newLink = "";
            bool isUnique = false;

            while (!isUnique)
            {
                var codeArray = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    // Random.Shared.Next() lấy ngẫu nhiên 1 vị trí trong chuỗi chars
                    codeArray[i] = chars[Random.Shared.Next(chars.Length)];
                }
                newLink = new string(codeArray);

                bool LinkExists = await _context.Classrooms.AnyAsync(c => c.InviteCode == newLink);

                if (!LinkExists)
                {
                    isUnique = true; // Mã duy nhất -> Dừng vòng lặp!
                }
            }

            return newLink;
        }
        public int DistanceLevenshtein( string s1, string s2){

            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            int l1 = s1.Length, l2 = s2.Length;
            int[,] dp = new int[l1 + 1,l2+1];
            for(int i=0;i<=l1;i++){
                dp[i,0]=i;             //đếm thao tác để ""  thành s1[1..i]
            }
            for(int i=0;i<=l2;i++){
                dp[0,i]=i;             // đếm thao tác để ""  thành s2[1..i]
            }
            int replace, dele, append;
            for (int i = 1; i <= l1; i++)
            {
                for (int j = 1; j <= l2; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    replace = dp[i - 1,j - 1] + cost;  // thao tác thay
                    dele = dp[i - 1,j] + 1;           // thao tác xóa
                    append = dp[i,j - 1] + 1;         // thao tác chèn thêm

                    int nho = Math.Min(replace, dele);
                    nho = Math.Min(nho, append);
                    dp[i,j] = nho;
                }
            }
            return dp[l1,l2];
        }
        
    }
}
