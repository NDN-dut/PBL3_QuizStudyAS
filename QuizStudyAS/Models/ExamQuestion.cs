namespace QuizStudyAS.Models
{
    public class ExamQuestion
    {
        public int QuestionId { get; set; }
        public int ExamId { get; set; } // Thuộc về bài kiểm tra nào
        public string Content { get; set; } // Nội dung câu hỏi
        public string? Explanation { get; set; } // Giải thích đáp án (nếu có)

        public virtual Exam Exam { get; set; }
        public virtual ICollection<QuestionOption> Options { get; set; }

        public ExamQuestion()
        {
            Options = new HashSet<QuestionOption>();
        }
    }
}
