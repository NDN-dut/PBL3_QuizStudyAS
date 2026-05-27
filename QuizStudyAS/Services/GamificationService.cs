using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.Models;

namespace QuizStudyAS.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly AppDbContext _context; // Thay bằng tên DbContext của bạn

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GamificationResultVM> UpdateUserProgressAsync(string userId, int earnedXP)
        {
            var result = new GamificationResultVM { Success = false };
            var user = await _context.Users.FindAsync(userId); // Thay Users bằng tên DbSet User của bạn

            if (user == null) return result;

            // ==========================================
            // 1. TÍNH TOÁN CHUỖI NGÀY HỌC (STREAK)
            // ==========================================
            var today = DateTime.Today;
            result.IsStreakSaved = false;

            if (user.LastStudyDate.HasValue)
            {
                var lastDate = user.LastStudyDate.Value.Date;

                if (lastDate == today.AddDays(-1))
                {
                    // Đã học hôm qua -> Cứu chuỗi thành công, cộng thêm 1 ngày
                    user.CurrentStreak++;
                    result.IsStreakSaved = true;
                }
                else if (lastDate < today.AddDays(-1))
                {
                    // Bỏ lỡ hơn 1 ngày -> Đứt chuỗi, quay về 1
                    user.CurrentStreak = 1;
                    result.IsStreakSaved = true;
                }
                // Nếu lastDate == today -> Người dùng học nhiều lần trong ngày, giữ nguyên chuỗi
            }
            else
            {
                // Lần đầu tiên học
                user.CurrentStreak = 1;
                result.IsStreakSaved = true;
            }

            // Cập nhật kỷ lục chuỗi
            if (user.CurrentStreak > user.HighestStreak)
            {
                user.HighestStreak = user.CurrentStreak;
            }

            user.LastStudyDate = DateTime.Now;
            // ==========================================
            // 2. TÍNH TOÁN XP VÀ CẤP ĐỘ (LEVEL UP)
            // ==========================================
            user.XP += earnedXP;
            result.IsLeveledUp = false;

            // ĐÃ ĐỔI THÀNH CỐ ĐỊNH 1000 XP = 1 CẤP
            int xpNeeded = 1000;

            // Vòng lặp while xử lý trường hợp user nhận quá nhiều XP
            while (user.XP >= xpNeeded)
            {
                user.XP -= xpNeeded; // Trừ đi số XP đã dùng để lên cấp
                user.Level++;        // Tăng cấp độ
                result.IsLeveledUp = true;
            }

            // Lưu vào CSDL
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Trả kết quả về cho Controller
            result.Success = true;
            result.EarnedXP = earnedXP;
            result.CurrentXP = user.XP;
            result.Level = user.Level;
            result.CurrentStreak = user.CurrentStreak;

            return result;
        }
    }
}