namespace QuizStudyAS.Models
{
    public class UserAchievement
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int AchievementId { get; set; }
        public virtual Achievement Achievement { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.Now; // Thời điểm mở khóa
    }
}