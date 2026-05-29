using Microsoft.EntityFrameworkCore;
using QuizStudyAS.Data;
using QuizStudyAS.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace QuizStudyAS.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _context;

        public LeaderboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeaderboardVM> GetLeaderboardAsync()
        {
            var allUsers = await _context.Users.Where(u => u.IsActive).ToListAsync();

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

            return new LeaderboardVM
            {
                LevelRanking = levelRank,
                StreakRanking = streakRank
            };
        }
    }
}