namespace QuizStudyAS.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Name { get; set; }          // Ví dụ: "Chăm chỉ I"
        public string Description { get; set; }   // Ví dụ: "Đạt chuỗi học 3 ngày liên tiếp"
        public string IconUrl { get; set; }       // Link ảnh hoặc class icon (VD: "bi bi-fire")
        public int RequiredXP { get; set; } = 0;  // Có thể thưởng thêm XP khi nhận thành tựu

        public virtual ICollection<UserAchievement> UserAchievements { get; set; }

        public Achievement()
        {
            UserAchievements = new HashSet<UserAchievement>();
        }
    }
}
