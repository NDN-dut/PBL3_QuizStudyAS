namespace QuizStudyAS.Models
{
    public class ClassroomUser
    {
        public int ClassroomId { get; set; } // Khóa chính + Khóa ngoại
        public string UserId { get; set; } // Khóa chính + Khóa ngoại
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        public virtual Classroom Classroom { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}