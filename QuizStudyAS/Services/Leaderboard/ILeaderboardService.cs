using QuizStudyAS.ViewModels;
using System.Threading.Tasks;

namespace QuizStudyAS.Services.Leaderboard
{
    public interface ILeaderboardService
    {
        Task<LeaderboardVM> GetLeaderboardAsync();
    }
}