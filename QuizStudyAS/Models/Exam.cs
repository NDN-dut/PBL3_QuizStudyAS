using System;
using System.Collections.Generic;

namespace QuizStudyAS.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public int ClassroomId { get; set; } // Khóa ngoại trỏ về phòng học
        public string Title { get; set; }
        public string? Description { get; set; }

        // Thời gian mở và đóng bài kiểm tra
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Thời lượng làm bài (tính bằng phút)
        public int DurationMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Classroom Classroom { get; set; }
        public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } // Danh sách câu hỏi của đề này
        public virtual ICollection<ExamAttempt> Attempts { get; set; } // Các lượt làm bài của thành viên

        public Exam()
        {
            ExamQuestions = new HashSet<ExamQuestion>();
            Attempts = new HashSet<ExamAttempt>();
        }
    }
}