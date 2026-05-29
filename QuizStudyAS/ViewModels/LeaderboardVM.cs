namespace QuizStudyAS.ViewModels
{
    public class UserRankVM
    {
        public int Rank { get; set; }
        public string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string ValueDisplay { get; set; }
        public int Level { get; set; }
        public int Streak { get; set; }
        public int XP { get; set; }
    }

    public class LeaderboardVM
    {
        public List<UserRankVM> LevelRanking { get; set; } = new();
        public List<UserRankVM> StreakRanking { get; set; } = new();
    }
}