using Microsoft.AspNetCore.Mvc;
using QuizStudyAS.Services.Leaderboard;
using System.Threading.Tasks;

namespace QuizStudyAS.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        public async Task<IActionResult> Index()
        {
            // Giao toàn bộ việc truy vấn DB cho Service
            var viewModel = await _leaderboardService.GetLeaderboardAsync();
            return View(viewModel);
        }
    }
}