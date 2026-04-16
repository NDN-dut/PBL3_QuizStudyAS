namespace QuizStudyAS.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Đổi UserId thành Id
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties (Liên kết) ---
        public virtual ICollection<StudySet> StudySets { get; set; }

        public virtual ICollection<Classroom> OwnedClassrooms { get; set; }
        public virtual ICollection<ClassroomUser> JoinedClassrooms { get; set; }
        public virtual ICollection<LearningProgress> LearningProgresses { get; set; }
        public virtual ICollection<GameSession> GameSessions { get; set; }

        public ApplicationUser()
        {
            StudySets = new HashSet<StudySet>();
            OwnedClassrooms = new HashSet<Classroom>();
            JoinedClassrooms = new HashSet<ClassroomUser>();
            LearningProgresses = new HashSet<LearningProgress>();
            GameSessions = new HashSet<GameSession>();
        }
    }
}