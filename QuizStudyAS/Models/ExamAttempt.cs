namespace QuizStudyAS.Models
{
    // Lưu thông tin 1 lượt thi của 1 user
    public class ExamAttempt
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }
        public string UserId { get; set; }

        public DateTime StartedAt { get; set; } // Bắt đầu lúc nào
        public DateTime? CompletedAt { get; set; } // Nộp bài lúc nào

        public double Score { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }

        public virtual Exam Exam { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<ExamAttemptDetail> Details { get; set; }

        public ExamAttempt()
        {
            Details = new HashSet<ExamAttemptDetail>();
        }
    }
}
