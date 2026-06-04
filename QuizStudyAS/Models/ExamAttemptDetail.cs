namespace QuizStudyAS.Models
{
    // Lưu lại user đã chọn đáp án nào cho câu hỏi nào
    public class ExamAttemptDetail
    {
        public int AttemptDetailId { get; set; }
        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; } // Null nếu user bỏ qua không chọn

        public virtual ExamAttempt Attempt { get; set; }
        public virtual ExamQuestion Question { get; set; }
        public virtual QuestionOption SelectedOption { get; set; }
    }
}
