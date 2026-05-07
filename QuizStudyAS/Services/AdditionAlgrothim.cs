using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
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
        
    }
}
