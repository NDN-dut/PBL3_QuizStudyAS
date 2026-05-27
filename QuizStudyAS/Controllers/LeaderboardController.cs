using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly AppDbContext _context;

        public LeaderboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ danh sách người dùng đang hoạt động
            var allUsers = await _context.Users.Where(u => u.IsActive).ToListAsync();

            // 1. Sắp xếp bảng Cấp độ (Level cao nhất -> XP cao nhất)
            var levelRank = allUsers
                .OrderByDescending(u => u.Level)
                .ThenByDescending(u => u.XP)
                .Select((u, idx) => new UserRankVM
                {
                    Rank = idx + 1,
                    UserName = u.UserName,
                    AvatarUrl = u.AvatarUrl,
                    Level = u.Level,
                    Streak = u.CurrentStreak,
                    XP = u.XP,
                    ValueDisplay = $"Cấp độ {u.Level} ({u.XP} XP)"
                }).ToList();

            // 2. Sắp xếp bảng Chuỗi ngày học (Streak cao nhất)
            var streakRank = allUsers
                .OrderByDescending(u => u.CurrentStreak)
                .Select((u, idx) => new UserRankVM
                {
                    Rank = idx + 1,
                    UserName = u.UserName,
                    AvatarUrl = u.AvatarUrl,
                    Level = u.Level,
                    Streak = u.CurrentStreak,
                    XP = u.XP,
                    ValueDisplay = $"{u.CurrentStreak} Ngày 🔥"
                }).ToList();

            var viewModel = new LeaderboardVM
            {
                LevelRanking = levelRank,
                StreakRanking = streakRank
            };

            return View(viewModel);
        }
    }
}
