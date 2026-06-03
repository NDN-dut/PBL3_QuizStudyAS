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

        public double Score { get; set; } // Điểm số
        public bool IsSubmitted { get; set; } // Đã nộp chưa (hay đang làm dở)
        public bool IsLate { get; set; } // BỔ SUNG DÒNG NÀY

        public virtual Exam Exam { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<ExamAttemptDetail> Details { get; set; }

        public ExamAttempt()
        {
            Details = new HashSet<ExamAttemptDetail>();
        }
    }
}
