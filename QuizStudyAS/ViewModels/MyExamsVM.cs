using System;
using System.Collections.Generic;

namespace QuizStudyAS.ViewModels
{
    public class MyExamsVM
    {
        public List<ExamItemVM> PendingExams { get; set; } = new List<ExamItemVM>();
        public List<ExamItemVM> CompletedExams { get; set; } = new List<ExamItemVM>();
    }

    public class ExamItemVM
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public string ClassName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }

        public double? Score { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }
        public bool IsMissed { get; set; } // Đánh dấu true nếu đã quá hạn mà chưa nộp
    }
}