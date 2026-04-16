namespace QuizStudyAS.Models
{
    public class QuizQuestionResult
    {
        public int ResultId { get; set; }
        public int SessionId { get; set; }
        public int FlashcardId { get; set; }

        public bool IsCorrect { get; set; } // Trả lời đúng hay sai

        // --- Navigation Properties ---
        public virtual GameSession GameSession { get; set; }
        public virtual Flashcard Flashcard { get; set; }
    }
}