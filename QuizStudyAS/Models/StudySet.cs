namespace QuizStudyAS.Models
{
    public class StudySet
    {
        public int StudySetId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string OwnerUserId { get; set; } // Khóa ngoại tới ApplicationUser
        public int? ClassroomId { get; set; } // Dấu '?' nghĩa là có thể Null (Nullable)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ICollection<Flashcard> Flashcards { get; set; }
        // Thêm dòng này để điều hướng ngược về Classroom
        public virtual Classroom? Classroom { get; set; }

        public StudySet()
        {
            Flashcards = new HashSet<Flashcard>();
        }
    }
}