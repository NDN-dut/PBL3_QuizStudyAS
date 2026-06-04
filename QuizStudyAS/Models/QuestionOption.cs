namespace QuizStudyAS.Models
{
    public class QuestionOption
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string Content { get; set; } // Nội dung đáp án (A, B, C, D...)
        public bool IsCorrect { get; set; } // Đánh dấu đáp án đúng

        public virtual ExamQuestion Question { get; set; }
    }
}
