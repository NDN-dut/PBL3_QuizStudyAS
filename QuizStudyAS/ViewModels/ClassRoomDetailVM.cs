using System;
using System.Collections.Generic;

namespace QuizStudyAS.ViewModels
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
    }

    // DTO nhỏ để chứa thông tin cơ bản của Bài kiểm tra hiển thị trong lớp
    public class ClassroomExamItemVM
    {
        public int ExamId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class ClassroomPostVM
    {
        public int PostId { get; set; }
        public string AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClassRoomDetailVM
    {
        public int ClassroomId { get; set; } // Dùng để truyền vào link Tạo đề (/Exam/Create?classroomId=...)
        public string ClassName { get; set; }
        public string OwnerName { get; set; }
        public string ClassCode { get; set; }
        public bool IsActive { get; set; }

        public bool IsOwner { get; set; } // Kiểm tra xem người đang xem có phải Chủ phòng không

        public List<StudySetItemVM> StudySets { get; set; }
        public string? ExistingAvatarUrl { get; set; }
        public List<UserInfo> ClassUsers { get; set; }

        // Danh sách Bài kiểm tra của lớp này
        public List<ClassroomExamItemVM> Exams { get; set; } = new List<ClassroomExamItemVM>();

        // Danh sách Thông báo của lớp học
        public List<ClassroomPostVM> Posts { get; set; } = new List<ClassroomPostVM>();
    }
}