namespace QuizStudyAS.Models
{
    public class GameSession
    {
        public int SessionId { get; set; }
        public string UserId { get; set; }
        public int StudySetId { get; set; }

        // 1: Trắc nghiệm (Quiz), 2: Nối từ (Match), 3: Sống sót (Survival)...
        public int GameType { get; set; }
        public int Score { get; set; }
        public int CompletionTime { get; set; } // Tính bằng giây, dùng để xếp hạng Leaderboard
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        public virtual ApplicationUser User { get; set; }
        public virtual StudySet StudySet { get; set; }
        public virtual ICollection<QuizQuestionResult> QuizQuestionResults { get; set; }

        public GameSession()
        {
            QuizQuestionResults = new HashSet<QuizQuestionResult>();
        }
    }
}