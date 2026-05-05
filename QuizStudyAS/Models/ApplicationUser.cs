namespace QuizStudyAS.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Đổi UserId thành Id
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true; // Mặc định tạo ra là được hoạt động

        // Thêm 2 cột để lưu mã Reset Password
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }

        // --- Navigation Properties (Liên kết) ---
        public virtual Role Role { get; set; }
        public virtual ICollection<StudySet> StudySets { get; set; }

        public virtual ICollection<Classroom> OwnedClassrooms { get; set; }
        public virtual ICollection<ClassroomUser> JoinedClassrooms { get; set; }
        public virtual ICollection<LearningProgress> LearningProgresses { get; set; }
        public virtual ICollection<GameSession> GameSessions { get; set; }

        public virtual ICollection<RequestJoinClass> JoinClassRooms {  get; set; }
        public ApplicationUser()
        {
            StudySets = new HashSet<StudySet>();
            OwnedClassrooms = new HashSet<Classroom>();
            JoinedClassrooms = new HashSet<ClassroomUser>();
            LearningProgresses = new HashSet<LearningProgress>();
            GameSessions = new HashSet<GameSession>();
            JoinClassRooms = new HashSet<RequestJoinClass>();
        }
    }
}