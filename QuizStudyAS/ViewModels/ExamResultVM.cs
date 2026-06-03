using System;
using System.Collections.Generic;

namespace QuizStudyAS.ViewModels
{
    public class ExamResultVM
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Danh sách thống kê từng học sinh
        public List<StudentResultItemVM> StudentResults { get; set; } = new List<StudentResultItemVM>();
    }

    public class StudentResultItemVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int? AttemptId { get; set; } // BỔ SUNG DÒNG NÀY
        public string Status { get; set; } // "Chưa làm", "Đang làm", "Đã nộp", "Nộp trễ", "Vắng thi"
        public string StatusColor { get; set; } // Màu badge bootstrap (success, warning, danger...)

        public double? Score { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}