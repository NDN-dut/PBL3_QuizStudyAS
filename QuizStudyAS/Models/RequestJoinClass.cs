namespace QuizStudyAS.Models
{
    public class RequestJoinClass
    {
        public string Status { get; set; }
        public string UserId { get; set; }
        public int ClassroomId { get; set; }

        // --- Navigation Properties ---
        public virtual ApplicationUser User { get; set; }
        public virtual Classroom Classroom { get; set; }
    }
}
