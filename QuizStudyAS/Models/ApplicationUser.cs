namespace QuizStudyAS.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public string? AvatarUrl { get; set; }

        // ==========================================
        // THÔNG SỐ GAMIFICATION (GAME HÓA)
        // ==========================================
        public int XP { get; set; } = 0;              // Điểm kinh nghiệm hiện tại
        public int Level { get; set; } = 1;           // Cấp độ (Bắt đầu từ lv 1)

        // Thông số chuỗi ngày học (Streak)
        public int CurrentStreak { get; set; } = 0;   // Chuỗi ngày hiện tại (Ví dụ: 7 ngày)
        public int HighestStreak { get; set; } = 0;   // Kỷ lục chuỗi ngày cao nhất từng đạt được
        public DateTime? LastStudyDate { get; set; }  // Mốc thời gian lần cuối cùng học bài

        // --- Navigation Properties (Liên kết) ---
        public virtual Role Role { get; set; }
        public virtual ICollection<StudySet> StudySets { get; set; }
        public virtual ICollection<Classroom> OwnedClassrooms { get; set; }
        public virtual ICollection<ClassroomUser> JoinedClassrooms { get; set; }
        public virtual ICollection<LearningProgress> LearningProgresses { get; set; }
        public virtual ICollection<GameSession> GameSessions { get; set; }
        public virtual ICollection<RequestJoinClass> JoinClassRooms { get; set; }

        // Bảng trung gian: Liên kết User với các Thành tựu họ đã đạt được
        public virtual ICollection<UserAchievement> UserAchievements { get; set; }

        public ApplicationUser()
        {
            StudySets = new HashSet<StudySet>();
            OwnedClassrooms = new HashSet<Classroom>();
            JoinedClassrooms = new HashSet<ClassroomUser>();
            LearningProgresses = new HashSet<LearningProgress>();
            GameSessions = new HashSet<GameSession>();
            JoinClassRooms = new HashSet<RequestJoinClass>();

            // Khởi tạo List thành tựu
            UserAchievements = new HashSet<UserAchievement>();
        }
    }
}