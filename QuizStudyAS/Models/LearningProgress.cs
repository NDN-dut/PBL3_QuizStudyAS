namespace QuizStudyAS.Models
{
    public class LearningProgress
    {
        public int ProgressId { get; set; }
        public string UserId { get; set; }
        public int FlashcardId { get; set; }

        public bool IsMastered { get; set; } = false; // Đã thuộc chưa?
        public int WrongCount { get; set; } = 0; // Số lần trả lời sai
        public DateTime LastReviewedAt { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        public virtual ApplicationUser User { get; set; }
        public virtual Flashcard Flashcard { get; set; }
    }
}