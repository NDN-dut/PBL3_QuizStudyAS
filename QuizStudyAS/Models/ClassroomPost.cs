namespace QuizStudyAS.Models
{
    public class ClassroomPost
    {
        public int PostId { get; set; }
        public int ClassroomId { get; set; }
        public string AuthorUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Classroom Classroom { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}
