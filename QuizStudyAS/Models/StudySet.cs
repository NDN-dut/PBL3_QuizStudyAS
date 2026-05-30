namespace QuizStudyAS.Models
{
    public class StudySet
    {
        public int StudySetId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public string OwnerUserId { get; set; } // Khóa ngoại tới ApplicationUser

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // --- Navigation Properties ---
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ICollection<Flashcard> Flashcards { get; set; }
        public virtual ICollection<ClassRoomMaterial> MaterialsOf { get; set; }

        public StudySet()
        {
            Flashcards = new HashSet<Flashcard>();
            MaterialsOf = new HashSet<ClassRoomMaterial>();
        }
    }
}