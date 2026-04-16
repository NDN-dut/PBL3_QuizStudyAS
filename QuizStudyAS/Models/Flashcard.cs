namespace QuizStudyAS.Models
{
    public class Flashcard
    {
        public int FlashcardId { get; set; }
        public int StudySetId { get; set; } // Khóa ngoại tới StudySet

        public string Term { get; set; }
        public string Definition { get; set; }

        public string? Example { get; set; } // Các trường có dấu '?' là Nullable
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        // --- Navigation Properties ---
        public virtual StudySet StudySet { get; set; }
    }
}