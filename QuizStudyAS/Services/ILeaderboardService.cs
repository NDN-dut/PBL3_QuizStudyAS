using QuizStudyAS.ViewModels;
using System.Threading.Tasks;

namespace QuizStudyAS.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardVM> GetLeaderboardAsync();
    }
}